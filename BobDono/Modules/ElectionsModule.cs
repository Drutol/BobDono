using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BobDono.Attributes;
using BobDono.BL;
using BobDono.Contexts;
using BobDono.Database;
using BobDono.Entities;
using BobDono.Utils;
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

        [CommandHandler(Regex = @"election create", Authorize = true, HumanReadableCommand = "election create",
            HelpText = "Starts new election.", Awaitable = false)]
        public async Task CreateElection(MessageCreateEventArgs args)
        {
            var cts = new CancellationTokenSource();
            var timeout = TimeSpan.FromMinutes(1);
            var member = await BotContext.DiscordClient.GetNullsGuild().GetMemberAsync(args.Author.Id);
            var channel = await member.CreateDmChannelAsync();           
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
                    int submissionDays = 0;
                    while (submissionDays <= 0)
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
                }
                catch (OperationCanceledException)
                {
                    return;
                }

            }
            finally
            {
                BotContext.NewPrivateMessage -= HandleQuit;
            }

            using (var context = new BobDatabaseContext())
            {
                context.Elections.Add(election);
                context.SaveChanges();
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
