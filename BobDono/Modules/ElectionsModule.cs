using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BobDono.Attributes;
using BobDono.BL;
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
        [CommandHandler(Regex = @"election create", Authorize = true, HumanReadableCommand = "election create",
            HelpText = "Starts new election.",Awaitable = false)]
        public async Task CreateElection(MessageCreateEventArgs args)
        {
            var cts = new CancellationTokenSource();
            var timeout = TimeSpan.FromMinutes(1);
            var member = await BotContext.DiscordClient.Guilds.First().Value.GetMemberAsync(args.Author.Id);
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
