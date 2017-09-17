using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace BobDono.Utils
{
    public static class ChannelExtensions
    {
        public static async Task<string> GetNextMessageAsync(this DiscordChannel channel, TimeSpan timeout, CancellationToken? token = null)
        {
            var completionSource = new TaskCompletionSource<string>();


            BotContext.NewPrivateMessage += Handler;
        
            try
            {
                return await completionSource.TimedAwait(timeout, token);
            }
            finally
            {
                BotContext.NewPrivateMessage -= Handler;
            }


            void Handler(MessageCreateEventArgs a) 
            {
                if(a.Channel.IsPrivate && a.Channel.Id == channel.Id && !a.Author.IsBot)
                    completionSource.SetResult(a.Message.Content);       
            }

        }

    }
}
