using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var member = await BotContext.DiscordClient.Guilds.First().Value.GetMemberAsync(args.Author.Id);
            
            var nameCompletionSource = new TaskCompletionSource<string>();

            var channel = await member.CreateDmChannelAsync();
            
            await channel.SendMessageAsync("You are about to create new election!\n\nProvide short name for it:");

            Action<MessageCreateEventArgs> handler = a => nameCompletionSource.SetResult(a.Message.Content);
            BobDono.ChannelRouteOverrides[channel.Id] = handler;
            var name = await nameCompletionSource.Task;
            BobDono.ChannelRouteOverrides.Remove(channel.Id);

            using (var context = new BobDatabaseContext())
            {
                context.Elections.Add(new Election {Name = name});
                context.SaveChanges();
            }
        }
    }
}
