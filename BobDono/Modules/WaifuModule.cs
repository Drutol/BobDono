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
        private readonly IUserService _userService;
        private readonly IWaifuService _waifuService;
        private readonly IBotContext _botContext;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly ITrueWaifuService _trueWaifuService;

        public WaifuModule(IUserService userService, IWaifuService waifuService, IBotContext botContext,
            IExceptionHandler exceptionHandler, ITrueWaifuService trueWaifuService)
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
            using (var userService = _userService.ObtainLifetimeHandle<UserService>(executionContext))
            using (var waifuService = _waifuService.ObtainLifetimeHandle<WaifuService>(executionContext))
            {
                var user = await userService.GetOrCreateUser(args.Author);


                var cts = new CancellationTokenSource();
                var timeout = TimeSpan.FromMinutes(1);
                var guild = ResourceLocator.DiscordClient.GetNullsGuild();
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
                            "Would you like to add image that fully conveys your waifu's superiority?");
                        var img = await channel.GetNextMessageAsync(timeout, cts.Token);
                        if (img == "none")
                            img = null;
                        else
                        {
                            //is it really a link?
                            if (!Regex.IsMatch(img,
                                @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)")
                            )
                                img = null;
                        }

                        var trueWaifu = new TrueWaifu
                        {
                            Description = description,
                            FeatureImage = img,
                            User = user,
                            Waifu = waifu,
                        };

                        user.TrueWaifu = trueWaifu;

                        await channel.SendMessageAsync(null, false, trueWaifu.GetEmbedBuilder());
                    }
                    catch (OperationCanceledException)
                    {
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

        [CommandHandler(Regex = @"waifu (<@\d+>|\w+)", HumanReadableCommand = "waifu <username>", HelpText = "Shows waifu of specified user.")]
        public async Task ViewWaifu(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var userService = _userService.ObtainLifetimeHandle<UserService>(executionContext))
            {
                var username = args.Message.GetSubject();
                userService.ConfigureIncludes().WithChain(query =>
                {
                    return query.Include(u => u.TrueWaifu)
                                .ThenInclude(w => w.Waifu);
                }).Commit();
                var user = await userService.FirstAsync(u => u.Name.ToLower().Contains(username.ToLower()));

                if (user == null)
                {
                    await args.Channel.SendMessageAsync("Couldn't find specified user.");
                    return;
                }

                if (user.TrueWaifu == null)
                {
                    await args.Channel.SendMessageAsync("Specified user didn't set his waifu yet. What a barbarian.");
                    return;
                }


                await args.Channel.SendMessageAsync(null, false, user.TrueWaifu.GetEmbedBuilder());
            }
        }


        [CommandHandler(Regex = "waifuremove", Awaitable = false, HumanReadableCommand = "waifuremove",
            HelpText = "Remove your waifu. :(")]
        public async Task RemoveWaifu(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var userService = _userService.ObtainLifetimeHandle<UserService>(executionContext))
            using (var trueWaifuService = _trueWaifuService.ObtainLifetimeHandle<TrueWaifuService>(executionContext))
            {
                userService.ConfigureIncludes().WithChain(query => query.Include(u => u.TrueWaifu)).Commit();
                var user = await userService.GetOrCreateUser(args.Author);

                trueWaifuService.Remove(user.TrueWaifu);
            }
        }

        [CommandHandler(Regex = "waifulist",HumanReadableCommand = "waifulist",HelpText = "List all waifus set by users.")]
        public async Task WaifuList(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var trueWaifuService = _trueWaifuService.ObtainLifetimeHandle<TrueWaifuService>(executionContext))
            {
                trueWaifuService.ConfigureIncludes().WithChain(q => q.Include(w => w.Waifu).Include(w => w.User)).Commit();

                var s = string.Join("\n",trueWaifuService.GetAll().Select(waifu =>
                    $"**{waifu.User.Name}** -- {waifu.Waifu.Name} *({waifu.Waifu.MalId})*"));

                await args.Channel.SendMessageAsync(s);
            }
        }
    }
}


