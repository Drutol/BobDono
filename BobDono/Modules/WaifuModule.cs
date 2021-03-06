﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BobDono.Core;
using BobDono.Core.Attributes;
using BobDono.Core.BL;
using BobDono.Core.Extensions;
using BobDono.Core.Interfaces;
using BobDono.Core.Utils;
using BobDono.DataAccess.Services;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;
using BobDono.MalHell.Queries;
using BobDono.Models.Entities;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Modules
{
    [Module(Name = "Waifu",Description = "Allows to manage your waifu and browse other low-tier waifus of fellow sever members.")]
    public class WaifuModule
    {
        private readonly IBotContext _botContext;
        private readonly IExceptionHandler _exceptionHandler;

        private readonly IServiceFactory<IUserService> _userService;
        private readonly IServiceFactory<IWaifuService> _waifuService;
        private readonly IServiceFactory<ITrueWaifuService> _trueWaifuService;

        public WaifuModule(IServiceFactory<IUserService> userService, IServiceFactory<IWaifuService> waifuService, IBotContext botContext,
            IExceptionHandler exceptionHandler, IServiceFactory<ITrueWaifuService> trueWaifuService)
        {
            _userService = userService;
            _waifuService = waifuService;
            _botContext = botContext;
            _exceptionHandler = exceptionHandler;
            _trueWaifuService = trueWaifuService;
        }


        [CommandHandler(Regex = "waifuset", Awaitable = false, HumanReadableCommand = "waifuset",
            HelpText = "Set your waifu.")]
        public async Task SetWaifu(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var userService = _userService.ObtainLifetimeHandle(executionContext))
            using (var waifuService = _waifuService.ObtainLifetimeHandle(executionContext))
            using (var truewWaifuService = _trueWaifuService.ObtainLifetimeHandle(executionContext))
            {
                userService.ConfigureIncludes().WithChain(query => query.Include(u => u.TrueWaifu)).Commit();
                var user = await userService.GetOrCreateUser(args.Author);


                var cts = new CancellationTokenSource();
                var timeout = TimeSpan.FromMinutes(2);
                var guild = ResourceLocator.DiscordClient.GetCurrentGuild();
                var member = await guild.GetMemberAsync(args.Author.Id);
                var channel = await member.CreateDmChannelAsync();

                try
                {
                    _botContext.NewPrivateMessage += HandleQuit;

                    try
                    {
                        //get waifu
                        var waifu = await channel.GetNextValidResponse("Provide MAL's id of your waifu:",
                            ParseWaifu,
                            timeout, cts.Token);

                        //get description
                        await channel.SendMessageAsync(
                            "Would you like to add something about your waifu? Type `none` to skip. 500 characters max.");
                        var description = await channel.GetNextMessageAsync(timeout, cts.Token);
                        description = description == "none" ? null : description.Substring(0, Math.Min(description.Length,500));

                        //get image
                        await channel.SendMessageAsync(
                            "Would you like change default MAL image? Type `none` to skip.");
                        var thumb = await channel.GetNextMessageAsync(timeout, cts.Token);
                        if (thumb == "none")
                            thumb = null;
                        else
                        {
                            //is it really a link?
                            if (!thumb.IsLink())
                                thumb = null;
                        }

                        //get image
                        await channel.SendMessageAsync(
                            "Would you like to add feature image that fully conveys your waifu's superiority? Type `none` to skip.");
                        var img = await channel.GetNextMessageAsync(timeout, cts.Token);
                        if (img == "none")
                            img = null;
                        else
                        {
                            //is it really a link?
                            if (!img.IsLink())
                                img = null;
                        }

                        if (user.TrueWaifu != null)
                        {
                            truewWaifuService.Remove(user.TrueWaifu);
                        }

                        var trueWaifu = new TrueWaifu
                        {
                            Description = description,
                            FeatureImage = img,
                            ThumbImage = thumb,
                            User = user,
                            Waifu = waifu,
                        };

                        user.TrueWaifu = trueWaifu;

                        await channel.SendMessageAsync(null, false, trueWaifu.GetEmbedBuilder());
                    }
                    catch (OperationCanceledException)
                    {
                        await channel.SendMessageAsync(
                            "Well... you left your waifu waiting... that's not nice of you.");
                        return;
                    }
                    catch (Exception e)
                    {
                        _exceptionHandler.Handle(e);
                    }

                    Task<Waifu> ParseWaifu(string s)
                    {
                        return waifuService.GetOrCreateWaifu(s);
                    }

                }
                finally
                {
                    _botContext.NewPrivateMessage -= HandleQuit;
                }

                void HandleQuit(MessageCreateEventArgs a)
                {
                    if (a.Channel.Id == channel.Id &&
                        a.Message.Content.Equals("quit", StringComparison.CurrentCultureIgnoreCase))
                    {
                        cts.Cancel();
                    }
                }
            }
        }

        [CommandHandler(Regex = "waifu",HumanReadableCommand = "waifu",HelpText = "Displays your waifu.")]
        public async Task Waifu(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var userService = _userService.ObtainLifetimeHandle(executionContext))
            {
                userService.ConfigureIncludes().WithChain(query =>
                {
                    return query.Include(u => u.TrueWaifu)
                        .ThenInclude(w => w.Waifu);
                }).Commit();
                var user = await userService.GetOrCreateUser(args.Author);

                await DisplayWaifuForUser(user, args.Channel);
            }
        }

        [CommandHandler(Regex = @"waifu (<@\d+>|\w+|<@!\d+>)", HumanReadableCommand = "waifu <username>", HelpText = "Shows waifu of specified user.")]
        public async Task ViewWaifu(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var userService = _userService.ObtainLifetimeHandle(executionContext))
            {

                userService.ConfigureIncludes().WithChain(query =>
                {
                    return query.Include(u => u.TrueWaifu)
                                .ThenInclude(w => w.Waifu);
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

                await DisplayWaifuForUser(user,args.Channel);
            }
        }

        private async Task DisplayWaifuForUser(User user,DiscordChannel channel)
        {
            if (user == null)
            {
                await channel.SendMessageAsync("Couldn't find specified user.");
                return;
            }

            if (user.TrueWaifu == null)
            {
                await channel.SendMessageAsync("Specified user didn't set his waifu yet. What a barbarian.");
                return;
            }


            await channel.SendMessageAsync(null, false, user.TrueWaifu.GetEmbedBuilder());
        }


        [CommandHandler(Regex = "waifuremove", Awaitable = false, HumanReadableCommand = "waifuremove",
            HelpText = "Remove your waifu. :(")]
        public async Task RemoveWaifu(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var userService = _userService.ObtainLifetimeHandle(executionContext))
            using (var trueWaifuService = _trueWaifuService.ObtainLifetimeHandle(executionContext))
            {
                userService.ConfigureIncludes().WithChain(query => query.Include(u => u.TrueWaifu)).Commit();
                var user = await userService.GetOrCreateUser(args.Author);
                if (user.TrueWaifu != null)
                {
                    trueWaifuService.Remove(user.TrueWaifu);
                    await args.Channel.SendMessageAsync("Your waifu is no more...");
                }
                else
                {
                    await args.Channel.SendMessageAsync("Your waifu can't get any more imaginary than this...");
                }


            }
        }

        [CommandHandler(Regex = "waifulist",HumanReadableCommand = "waifulist",HelpText = "List all waifus set by users.")]
        public async Task WaifuList(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var trueWaifuService = _trueWaifuService.ObtainLifetimeHandle(executionContext))
            {
                trueWaifuService.ConfigureIncludes().WithChain(q => q.Include(w => w.Waifu).Include(w => w.User)).Commit();

                var waifus = trueWaifuService.GetAll();
                
                var s = string.Join("\n", waifus.Where(w => w.User != null && w.Waifu != null).Select(waifu =>
                    $"**{waifu.User.Name}** -- {waifu.Waifu.Name} *({waifu.Waifu.MalId})*"));

                await args.Channel.SendMessageAsync(s);
            }
        }

        [CommandHandler(Regex = @"waifuedit (thumb|note|feature) .*",HumanReadableCommand = "waifuedit thumb/desc/feature/ <value>",HelpText = "Edits... your... waifu? Am I correct? かな\nYou can type `none` for description to remove it, keep in mind that descriprion is 500 characters long.")]
        public async Task WaifuEdit(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var userService = _userService.ObtainLifetimeHandle(executionContext))
            using (var trueWaifuService = _trueWaifuService.ObtainLifetimeHandle(executionContext))
            {
                userService.ConfigureIncludes().WithChain(query => query.Include(u => u.TrueWaifu)).Commit();
                var user = await userService.GetOrCreateUser(args.Author);

                if (user.TrueWaifu == null)
                {
                    await args.Channel.SendMessageAsync("You need to set your waifu first in order to edit it... ne?");
                    return;
                }

                var param = args.Message.Content.Split(' ');

                if (param.Length < 3)
                {
                    await args.Message.CreateReactionAsync(DiscordEmoji.FromName(ResourceLocator.DiscordClient, ":humm:"));
                    return;
                }

                switch (param[1])
                {
                    case "thumb":
                        if(param[2] == "none")
                            user.TrueWaifu.ThumbImage = null;
                        else if (param[2].IsLink())
                            user.TrueWaifu.ThumbImage = param[2];
                        break;
                    case "note":
                        user.TrueWaifu.Description = param[2] == "none" ? null : args.Message.Content.Substring(17);
                        if (user.TrueWaifu.Description?.Length > 500)
                        {
                            user.TrueWaifu.Description = user.TrueWaifu.Description.Substring(0, 500);
                        }
                        break;
                    case "feature":
                        if (param[2] == "none")
                            user.TrueWaifu.FeatureImage = null;
                        else if(param[2].IsLink())
                            user.TrueWaifu.FeatureImage = param[2];
                        break;
                }

                trueWaifuService.ConfigureIncludes().WithChain(q => q.Include(w => w.Waifu).Include(w => w.User)).Commit();
                var waifu = await trueWaifuService.FirstAsync(w => w.User.Id == user.Id);

                await args.Channel.SendMessageAsync(null,false,waifu.GetEmbedBuilder());
            }         
        }
    }
}


