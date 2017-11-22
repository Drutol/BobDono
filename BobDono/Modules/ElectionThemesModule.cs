using System;
using System.Collections.Generic;
using System.Linq;
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
using BobDono.Models.Entities;
using BobDono.Models.Entities.Simple;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace BobDono.Modules
{    
    [Module(Hidden = true)]
    public class ElectionThemesModule
    {
        private readonly IElectionThemesChannelService _electionThemesChannelService;
        private readonly CustomDiscordClient _discordClient;


        public List<ElectionThemesContext> ElectionThemesContexts { get; } = new List<ElectionThemesContext>();

        public ElectionThemesModule(IElectionThemesChannelService electionThemesChannelService, DiscordClient discordClient)
        {
            _electionThemesChannelService = electionThemesChannelService;
            _discordClient = discordClient as CustomDiscordClient;

            InitializeExistingChannels();
        }

        private void InitializeExistingChannels()
        {
            using (var dependencyScope = ResourceLocator.ObtainScope())
            using (var electionthemeChannelService =
                _electionThemesChannelService.ObtainLifetimeHandle(ResourceLocator.ExecutionContext))
            {
                foreach (var channel in electionthemeChannelService.GetAll())
                {
                    try
                    {
                        var ctx = dependencyScope.Resolve<ElectionThemesContext>(
                            new TypedParameter(typeof(ElectionThemeChannel), channel));
                        ElectionThemesContexts.Add(ctx);
                        ctx.OnTimePass();
                    }
                    catch (Exception) //couldn't create election -> channel removed
                    {
                        //we will mark is as closed                       
                    }
                }
            }
        }

        [CommandHandler(Regex = "ethemescreate", Debug = true)]
        public async Task CreateElectionThemesChannel(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            ElectionThemesContext ctx = null;
            using (var dependencyScope = ResourceLocator.ObtainScope())
            using (var channelService = _electionThemesChannelService.ObtainLifetimeHandle(executionContext))
            {
                var guild = _discordClient.GetNullsGuild();
                var category = await guild.GetCategoryChannel(DiscordClientExtensions.ChannelCategory.ElectionsMeta);
                var electionChannel = await guild.CreateChannelAsync("Election-Themes", ChannelType.Text,
                    category);
                _discordClient.CreatedChannels.Add(electionChannel);

                var themeChannel = new ElectionThemeChannel
                {
                    DiscordChannelId = (long) electionChannel.Id,
                    NextElection = DateTime.Today.GetNextElectionDate(),
                };

                channelService.Add(themeChannel);
                ctx = dependencyScope.Resolve<ElectionThemesContext>(
                    new TypedParameter(typeof(ElectionThemeChannel), themeChannel));
                ElectionThemesContexts.Add(ctx);
                themeChannel.OpeningMessageId = await ctx.OnCreate(themeChannel);
            }
        }
    }
}
