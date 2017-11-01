using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using BobDono.Contexts;
using BobDono.Core;
using BobDono.Core.Attributes;
using BobDono.Core.BL;
using BobDono.Core.Extensions;
using BobDono.Core.Interfaces;
using BobDono.Core.Utils;
using BobDono.DataAccess.Database;
using BobDono.DataAccess.Services;
using BobDono.Interfaces;
using BobDono.Models.Entities;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Modules
{
    [Module]
    public class ElectionsModule
    {
        private readonly IUserService _userService;
        private readonly IBotContext _botContext;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly IElectionService _electionService;
        private readonly List<ElectionContext> _electionsContexts = new List<ElectionContext>();

        public ElectionsModule(IUserService userService,IBotContext botContext, IExceptionHandler exceptionHandler, IElectionService  electionService)
        {
            _userService = userService;
            _botContext = botContext;
            _exceptionHandler = exceptionHandler;
            _electionService = electionService;

            InitializeExistingElections();
        }

        private void InitializeExistingElections()
        {
            using (var dependencyScope = ResourceLocator.ObtainScope())
            {
                foreach (var election in _electionService.OneShot(() => _electionService.GetAll()))
                {
                    try
                    {
                        _electionsContexts.Add(
                            dependencyScope.Resolve<ElectionContext>(new TypedParameter(typeof(Election),
                                election)));
                    }
                    catch (Exception)
                    {
                        _electionService.OneShot(() => _electionService.Remove(election));
                    }
                }
            }
        }

        [CommandHandler(
            Regex = @"election create", 
            Authorize = true, 
            HumanReadableCommand = "election create",
            HelpText = "Starts new election.", 
            Awaitable = false)]
        public async Task CreateElection(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var userService = _userService.ObtainLifetimeHandle<UserService>(executionContext))
            using (var electionService = _electionService.ObtainLifetimeHandle<ElectionService>(executionContext))
            {
                var user = await userService.GetOrCreateUser(args.Author);


                var cts = new CancellationTokenSource();
                var timeout = TimeSpan.FromMinutes(1);
                var guild = ResourceLocator.DiscordClient.GetNullsGuild();
                var member = await guild.GetMemberAsync(args.Author.Id);
                var channel = await member.CreateDmChannelAsync();
                await channel.SendMessageAsync(
                    "You are about to create new election, you can always cancel by typing `quit`.\nProvide short name for it:");

                var election = new Election();
                try
                {
                    _botContext.NewPrivateMessage += HandleQuit;

                    try
                    {

                        election.Name = await channel.GetNextMessageAsync(timeout, cts.Token);
                        await channel.SendMessageAsync("Longer description if you could:");
                        election.Description = await channel.GetNextMessageAsync(timeout, cts.Token);
                        int submissionDays = 2;
                        while (submissionDays == 0)
                        {
                            await channel.SendMessageAsync(
                                "How long would you like the submission period to be? (1-7) days.");
                            var response = await channel.GetNextMessageAsync(timeout, cts.Token);
                            if (int.TryParse(response, out int days))
                            {
                                if (days >= 1 || days <= 7)
                                {
                                    submissionDays = days;
                                }
                            }
                        }
                        election.SubmissionsStartDate = DateTime.UtcNow;
                        election.SubmissionsEndDate = DateTime.UtcNow.AddDays(submissionDays);
                        election.VotingStartDate = election.SubmissionsEndDate.AddHours(2); //TODO Maybe add commands?

                        int submissionCount = 2;
                        while (submissionCount == 0)
                        {
                            await channel.SendMessageAsync(
                                "How many contestants can be submitted by one person? (1-9)");
                            var response = await channel.GetNextMessageAsync(timeout, cts.Token);
                            if (int.TryParse(response, out int count))
                            {
                                if (count >= 1 || count <= 7)
                                {
                                    submissionCount = count;
                                }
                            }
                        }
                        election.EntrantsPerUser = submissionCount;

                        await channel.SendMessageAsync(
                            "That'd be evrything I guess, let the wars begin. I'll now create a new battlefield!");
                        try
                        {
                            var category = await guild.GetElectionsCategory();
                            var electionChannel = await guild.CreateChannelAsync(election.Name, ChannelType.Text,
                                category,
                                null, null, null,
                                election.Description);
                            election.DiscordChannelId = electionChannel.Id;
                        }
                        catch (Exception e)
                        {

                        }



                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (Exception e)
                    {
                        _exceptionHandler.Handle(e);
                    }

                }
                finally
                {
                    _botContext.NewPrivateMessage -= HandleQuit;
                }

                await Task.Delay(1000);

                try
                {
                    using (var dependencyScope = ResourceLocator.ObtainScope())
                    {
                        var electionContext = dependencyScope.Resolve<ElectionContext>(new TypedParameter(typeof(Election),
                            await electionService.CreateElection(election, user)));
                        electionContext.OnCreated();
                        _electionsContexts.Add(electionContext);
                    }
                }
                catch (Exception e)
                {

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
    }
}
