using System;
using System.Threading;
using System.Threading.Tasks;
using BobDono.Core.Utils;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace BobDono.Core.Extensions
{
    public static class ChannelExtensions
    {
        public static async Task<string> GetNextMessageAsync(this DiscordChannel channel, TimeSpan timeout, CancellationToken? token = null)
        {
            var completionSource = new TaskCompletionSource<string>();


            ResourceLocator.BotContext.NewPrivateMessage += Handler;
        
            try
            {
                return await completionSource.TimedAwait(timeout, token);
            }
            finally
            {
                ResourceLocator.BotContext.NewPrivateMessage -= Handler;
            }


            void Handler(MessageCreateEventArgs a) 
            {
                if(a.Channel.IsPrivate && a.Channel.Id == channel.Id && !a.Author.IsBot)
                    completionSource.SetResult(a.Message.Content);       
            }

        }

    }
}
