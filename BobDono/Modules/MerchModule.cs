using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BobDono.Core.Attributes;
using BobDono.Core.Extensions;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;
using BobDono.Models.Entities;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Modules
{
    [Module(Name = "Merch",Description = "Show off your merch!")]
    public class MerchModule
    {
        private readonly IServiceFactory<IUserService> _userService;
        private readonly IServiceFactory<IMerchService> _merchService;

        public MerchModule(IServiceFactory<IUserService> userService, IServiceFactory<IMerchService> merchService)
        {
            _userService = userService;
            _merchService = merchService;
        }

        [CommandHandler(Regex = @"set merch .*\n.*\n.*",HumanReadableCommand = "set merch <pictureLink>\\n<name>\\n<description>",HelpText = "Show your favourite piece of ~~merchandise~~ art that you physically own!")]
        public async Task SetMerch(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var us = _userService.ObtainLifetimeHandle())
            {
                us.ConfigureIncludes().WithChain(query => query.Include(u => u.OwnedMerchandiseItems)).Commit();
                var user = await us.GetOrCreateUser(args.Author);

                var command = args.Message.Content.Replace($"{CommandHandlerAttribute.CommandStarter}set merch ", "");
                var pos = command.IndexOf('\n');

                var link = command.Substring(0,pos);
                if (!link.IsLink())
                {
                    await args.Channel.SendMessageAsync("Provided link is invalid.");
                }
                else
                {
                    if (user.OwnedMerchandiseItems.Any())
                    {
                        user.OwnedMerchandiseItems.Clear();
                    }

                    var pos2 = command.IndexOf('\n', pos + 1);

                    var item = new MerchandiseItem
                    {
                        ImageLink = link,
                        Name = command.Substring(pos + 1, pos2 - pos).Trim(),
                        Notes = command.Substring(pos2 + 1).Trim(),
                        Owner = user
                    };

                    if (item.Name.Length > 50)
                    {
                        await args.Channel.SendMessageAsync("Your title is too long. (50 character max)");
                        return;
                    }

                    if (item.Notes.Length > 1000)
                    {
                        await args.Channel.SendMessageAsync("Your description is too long. (1000 character max)");
                        return;
                    }

                    user.OwnedMerchandiseItems.Add(item);

                }

                await args.Channel.SendMessageAsync("You have added your merch successfully.");
            }
        }

        [CommandHandler(Regex = "merch", HumanReadableCommand = "merch", HelpText = "Displays your merch.")]
        public async Task ViewMerch(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var userService = _userService.ObtainLifetimeHandle(executionContext))
            {
                userService.ConfigureIncludes().WithChain(query =>
                    {
                        return query.Include(u => u.OwnedMerchandiseItems);
                    }).Commit();
                var user = await userService.GetOrCreateUser(args.Author);

                await DisplayMerchForUser(user, args.Channel);
            }
        }

        [CommandHandler(Regex = "merchlist", HumanReadableCommand = "merchlist", HelpText = "Displays users with set merch.")]
        public async Task MerchList(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var merchService = _merchService.ObtainLifetimeHandle(executionContext))
            {
                merchService.ConfigureIncludes().WithChain(q => q.Include(w => w.Owner)).Commit();
                var merch = merchService.GetAll();
                if (merch.Any())
                {
                    var s = "**All found merch:**\n\n";
                    s += string.Join("\n", merch.Where(m => m.Owner != null).Select(m =>
                        $"*{m.Owner.Name}* - {m.Name}"));
                    await args.Channel.SendMessageAsync(s);
                }
                else
                {
                    await args.Channel.SendMessageAsync("Nobody added their merch yet :(");
                }

            }
        }

        [CommandHandler(Regex = @"merch (<@\d+>|\w+|<@!\d+>)", HumanReadableCommand = "merch <username>", HelpText = "Shows merch of specified user.")]
        public async Task ViewOthersMerch(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var userService = _userService.ObtainLifetimeHandle(executionContext))
            {

                userService.ConfigureIncludes().WithChain(query =>
                    {
                        return query.Include(u => u.OwnedMerchandiseItems);
                    }).Commit();

                User user = null;
                if (args.Message.GetSubject(out var username))
                {
                    user = await userService.FirstAsync(u => u.Name.ToLower().Contains(username.ToLower()));
                }
                else
                {
                    user = await userService.FirstAsync(u => u.DiscordId == args.Message.MentionedUsers.First().Id);
                }

                await DisplayMerchForUser(user, args.Channel);
            }
        }

        private async Task DisplayMerchForUser(User user, DiscordChannel argsChannel)
        {
            if (user.OwnedMerchandiseItems.Any())
            {
                var embed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = user.AvatarUrl, Name = user.Name },
                    ImageUrl = user.OwnedMerchandiseItems.First().ImageLink,
                    Title = user.OwnedMerchandiseItems.First().Name,
                    Description = user.OwnedMerchandiseItems.First().Notes,
                    Color = DiscordColor.Orange
                };

                await argsChannel.SendMessageAsync(null, false, embed);
            }
            else
            {
                await argsChannel.SendMessageAsync("This user didn't add any merch.");
            }

        }
    }
}
