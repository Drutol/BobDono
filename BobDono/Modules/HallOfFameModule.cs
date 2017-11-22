using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using BobDono.Contexts;
using BobDono.Core;
using BobDono.Core.Attributes;
using BobDono.Core.Extensions;
using BobDono.Core.Utils;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;
using BobDono.Models.Entities.Simple;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace BobDono.Modules
{
    [Module(Hidden = true)]
    public class HallOfFameModule
    {
        private readonly IHallOfFameChannelService _hallOfFameChannelService;
        private readonly CustomDiscordClient _discordClient;

        public List<HallOfFameContext> HallOfFameContexts { get; } = new List<HallOfFameContext>();

        public HallOfFameModule(IHallOfFameChannelService hallOfFameChannelService, DiscordClient discordClient)
        {
            _hallOfFameChannelService = hallOfFameChannelService;
            _discordClient = discordClient as CustomDiscordClient;

            InitializeExistingChannels();
        }

        private void InitializeExistingChannels()
        {
            using (var dependencyScope = ResourceLocator.ObtainScope())
            using (var electionthemeChannelService =
                _hallOfFameChannelService.ObtainLifetimeHandle(ResourceLocator.ExecutionContext))
            {
                foreach (var channel in electionthemeChannelService.GetAll())
                {
                    try
                    {
                        HallOfFameContexts.Add(
                            dependencyScope.Resolve<HallOfFameContext>(new TypedParameter(typeof(HallOfFameChannel), channel)));
                    }
                    catch (Exception) //couldn't create election -> channel removed
                    {
                        //we will mark is as closed                       
                    }
                }
            }
        }

        [CommandHandler(Regex = "hofcreate",Debug = true)]
        public async Task CreateHallOfFame(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var dependencyScope = ResourceLocator.ObtainScope())
            using (var channelService = _hallOfFameChannelService.ObtainLifetimeHandle(executionContext))
            {
                var guild = _discordClient.GetNullsGuild();
                var category = await guild.GetCategoryChannel(DiscordClientExtensions.ChannelCategory.ElectionsMeta);
                var hallOfFameChannel = await guild.CreateChannelAsync("Hall-Of-Fame", ChannelType.Text,
                    category);
                _discordClient.CreatedChannels.Add(hallOfFameChannel);

                var hofChannel = new HallOfFameChannel
                {
                    DiscordChannelId = (long)hallOfFameChannel.Id,
                };

                channelService.Add(hofChannel);
                var ctx = dependencyScope.Resolve<HallOfFameContext>(
                    new TypedParameter(typeof(HallOfFameChannel), hofChannel));
                HallOfFameContexts.Add(ctx);
                hofChannel.OpeningMessageId = await ctx.OnCreate();
            }
        }
    }
}
