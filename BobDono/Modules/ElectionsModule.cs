using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BobDono.Attributes;
using BobDono.BL;
using BobDono.BL.Services;
using BobDono.Contexts;
using BobDono.Database;
using BobDono.Entities;
using BobDono.Utils;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace BobDono.Modules
{
    [Module]
    public class ElectionsModule
    {
        private List<ElectionContext> _electionsContexts;

        public ElectionsModule()
        {
            Task.Run(() =>
                {
                    using (var db = new BobDatabaseContext())
                    {
                        _electionsContexts = db.Elections
                            .Select(election => new ElectionContext(election))
                            .ToList();
                    }
                });
        }

        [CommandHandler(
            Regex = @"election create", 
            Authorize = true, 
            HumanReadableCommand = "election create",
            HelpText = "Starts new election.", 
            Awaitable = false)]
        public async Task CreateElection(MessageCreateEventArgs args)
        {
            var cts = new CancellationTokenSource();
            var timeout = TimeSpan.FromMinutes(1);
            var guild = BotContext.DiscordClient.GetNullsGuild();
            var member = await guild.GetMemberAsync(args.Author.Id);
            var channel = await member.CreateDmChannelAsync();
            var user = await UserService.Instance.GetOrCreateUser(args.Author);
            await channel.SendMessageAsync("You are about to create new election, you can always cancel by typing `quit`.\nProvide short name for it:");

            var election = new Election();
            try
            {
                BotContext.NewPrivateMessage += HandleQuit;

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
                    election.SubmissionsStartDate = DateTime.Now;
                    election.SubmissionsEndDate = DateTime.Now.AddDays(submissionDays);
                    election.SubmissionsStartDate = election.SubmissionsEndDate.AddHours(2); //TODO Maybe add commands?

                    int submissionCount = 2;
                    while (submissionCount == 0)
                    {
                        await channel.SendMessageAsync("How many contestants can be submitted by one person? (1-9)");
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

                    var category = await guild.GetElectionsCategory();
                    var electionChannel = await guild.CreateChannelAsync(election.Name, ChannelType.Text, category,
                        null, null, null,
                        election.Description);

                    election.DiscordChannelId = electionChannel.Id;
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception e)
                {
                    BotContext.ExceptionHandler.Handle(e);
                }

            }
            finally
            {
                BotContext.NewPrivateMessage -= HandleQuit;
            }

            await ElectionService.Instance.CreateElection(election, user);

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
