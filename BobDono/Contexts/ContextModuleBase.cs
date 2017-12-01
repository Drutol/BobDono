using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BobDono.Core;
using BobDono.Core.BL;
using BobDono.Core.Extensions;
using BobDono.Core.Interfaces;
using BobDono.Interfaces;
using DSharpPlus.Entities;

namespace BobDono.Contexts
{
    public abstract class ContextModuleBase : IModule
    {
        public ulong? ChannelIdContext { get; set; }
        public abstract DiscordChannel Channel { get; }

        public static List<ulong> ContextChannels { get; } = new List<ulong>();

        protected ContextModuleBase(ulong channelId)
        {
            ChannelIdContext = channelId;
            ContextChannels.Add(channelId);
            ResourceLocator.BotBackbone.Modules[GetType()].Contexts.Add(this);
        }

        protected async void ClearChannel()
        {
            var messages = await Channel.GetMessagesAsync();

            foreach (var message in messages)
            {
                try
                {
                    if (!message.Author.IsMe())
                        await message.DeleteAsync();
                }
                catch
                {

                }

            }
        }
    }
}
