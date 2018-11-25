using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using BobDono.Contexts;
using BobDono.Core;
using BobDono.Core.Attributes;
using BobDono.Core.BL;
using BobDono.Core.Extensions;
using BobDono.Core.Utils;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;
using BobDono.Models.Entities;
using BobDono.Models.Entities.Simple;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Modules
{
    [Module(Name = "HallOfFame",Description = "Commands to interact with hall of fame, like using winners commands.")]
    public class HallOfFameModule
    {
        private readonly IServiceFactory<IHallOfFameChannelService> _hallOfFameChannelService;
        private readonly IServiceFactory<IHallOfFameMemberService> _hallOfFameMemberService;
        private readonly IServiceFactory<IElectionService> _electionService;

        private readonly CustomDiscordClient _discordClient;

        public List<HallOfFameContext> HallOfFameContexts { get; } = new List<HallOfFameContext>();

        public HallOfFameModule(IServiceFactory<IHallOfFameChannelService> hallOfFameChannelService,
            DiscordClient discordClient, IServiceFactory<IHallOfFameMemberService> hallOfFameMemberService,
            IServiceFactory<IElectionService> electionService)
        {
            _hallOfFameChannelService = hallOfFameChannelService;
            _hallOfFameMemberService = hallOfFameMemberService;
            _electionService = electionService;
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


        [CommandHandler(Regex = @"hof \w+",HumanReadableCommand = "hof <userDefinedCommand>",HelpText = "Sends image set by owner to chat.")]
        public async Task HofImage(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var hofMembersService = _hallOfFameMemberService.ObtainLifetimeHandle(executionContext))
            {
                var param = args.Message.Content.Split(' ');

                hofMembersService.ConfigureIncludes().WithChain(query => query.Include(m => m.Contender.Proposer).Include(m => m.Contender.Waifu)).Commit();
                var member = await hofMembersService.FirstAsync(fameMember => fameMember.CommandName != null &&
                    fameMember.CommandName.Equals(param[1], StringComparison.CurrentCultureIgnoreCase));

                if (member != null)
                {
                    var embed = new DiscordEmbedBuilder();
                    embed.Color = DiscordColor.Gold;
                    embed.ImageUrl = member.ImageUrl;
                    embed.Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text =
                            $"{member.ElectionName} - {member.Contender.Proposer.Name} - ({member.Contender.Waifu.MalId})"
                    };
                    await args.Channel.SendMessageAsync(null, false, embed);
                }
                else
                {
                    await args.Channel.SendMessageAsync($"Didn't found such defined command. You can see all defined commands using `{CommandHandlerAttribute.CommandStarter}hofcmds`");
                }
            }
        }

        [CommandHandler(Regex = "hofcmds",HumanReadableCommand = "hofcmds",HelpText = "Displays all defined commands for winners.")]
        public async Task HallOfFameCommands(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var hofMembersService = _hallOfFameMemberService.ObtainLifetimeHandle(executionContext))
            {
                var members = hofMembersService.GetAll();

                if (members.Any(member => !string.IsNullOrEmpty(member.CommandName)))
                {
                    var output = "All defined commands:\n";
                    foreach (var hallOfFameMember in members.Where(member => !string.IsNullOrEmpty(member.CommandName)))
                    {
                        output += $"`{CommandHandlerAttribute.CommandStarter}hof {hallOfFameMember.CommandName}`\n";
                    }
                    await args.Channel.SendMessageAsync(output);
                }
                else
                {
                    await args.Channel.SendMessageAsync("No commands defined yet.");
                }
            }
        }

        [CommandHandler(Regex = "plzaddholo",Debug = true)]
        public async Task AddWinnersOfPastElectionToHallOfFame(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var electionService = _electionService.ObtainLifetimeHandle(executionContext))
            using (var hofMembersService = _hallOfFameMemberService.ObtainLifetimeHandle(executionContext))
            {
                var winningContenders = new List<(string Name, WaifuContender Winner,DateTime EndDate)>();

                foreach (var election in await electionService.GetAllWhereAsync(election =>
                    election.CurrentState == Election.State.Closed))
                {
                    var stage = election.BracketStages.Last();
                    winningContenders.Add((election.Name,stage.Brackets.Last().Winner,stage.EndDate));
                }

                hofMembersService.ConfigureIncludes().WithChain(query => query.Include(m => m.Contender)).Commit();
                var hofMembers = hofMembersService.GetAll();
                foreach (var winningContender in winningContenders)
                {
                    if (hofMembers.Any(member => member.Contender.Equals(winningContender.Winner)))
                        continue;

                    var hofMember = new HallOfFameMember
                    {
                        Contender = winningContender.Winner,
                        ElectionName = winningContender.Name,
                        WinDate = winningContender.EndDate,
                        Owner = winningContender.Winner.Proposer,
                    };

                    hofMembersService.Add(hofMember);

                    await Messenger.Instance.SendAsync(new HallOfFameContext.NewHofEntryMessage {Member = hofMember});
                }
            }
        }

        [CommandHandler(Regex = "hofcreate",Debug = true)]
        public async Task CreateHallOfFame(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var dependencyScope = ResourceLocator.ObtainScope())
            using (var channelService = _hallOfFameChannelService.ObtainLifetimeHandle(executionContext))
            {
                var guild = _discordClient.GetCurrentGuild();
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
