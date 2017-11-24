using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BobDono.Core;
using BobDono.Core.Attributes;
using BobDono.Core.BL;
using BobDono.Core.Extensions;
using BobDono.Core.Utils;
using BobDono.Interfaces;
using BobDono.Models.Entities.Simple;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace BobDono.Contexts
{
    [Module(Hidden = true,IsChannelContextual = true)]
    public class HallOfFameContext : ContextModuleBase
    {
        private readonly CustomDiscordClient _discordClient;
        private readonly DiscordChannel _channel;

        public override DiscordChannel Channel => _channel;

        public HallOfFameContext(HallOfFameChannel channel, DiscordClient discordClient) : base((ulong)channel.DiscordChannelId)
        {
            _discordClient = discordClient as CustomDiscordClient;
            ChannelIdContext = (ulong) channel.DiscordChannelId;

            var guild = ResourceLocator.DiscordClient.GetNullsGuild();
            _channel = _discordClient.GetChannel(guild, (ulong)channel.DiscordChannelId);

            if (_channel == null)
                throw new InvalidOperationException("Discord channel is invalid");

            ClearChannel();
        }

        [CommandHandler(FallbackCommand = true)]
        public async Task FallbackCommand(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            if (!args.Author.IsMe())
                await args.Message.DeleteAsync();
        }


        public async Task<long> OnCreate()
        {
            var embed = new DiscordEmbedBuilder();

            embed.Title = "What's going on?";
            embed.Description =
                "We will be keeping here all winners of past election.";

            embed.Color = DiscordColor.Gray;
            return (long)(await _channel.SendMessageAsync(null, false, embed.Build())).Id;
        }
    }
}
