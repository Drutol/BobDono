using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BobDono.Core;
using BobDono.Core.Attributes;
using BobDono.Core.BL;
using BobDono.Core.Extensions;
using BobDono.Core.Utils;
using BobDono.DataAccess.Services;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;
using BobDono.Models.Entities;
using BobDono.Models.Entities.Simple;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Contexts
{
    [Module(Name = "HallOfFame(Channel)",Description = "You can add commands here.",IsChannelContextual = true)]
    public class HallOfFameContext : ContextModuleBase
    {
        private const string CommandKey = "Command";
        private const string ElectionName = "Election";
        

        private readonly CustomDiscordClient _discordClient;
        private readonly DiscordChannel _channel;
        private readonly HallOfFameChannel _hofChannel;
        private readonly IElectionService _electionService;
        private readonly IUserService _userService;
        private readonly IHallOfFameMemberService _hallOfFameMemberService;

        public override DiscordChannel Channel => _channel;

        public HallOfFameContext(HallOfFameChannel channel, DiscordClient discordClient,IElectionService electionService, IUserService userService, IHallOfFameMemberService hallOfFameMemberService) : base((ulong)channel.DiscordChannelId)
        {
            _hofChannel = channel;
            _electionService = electionService;
            _userService = userService;
            _hallOfFameMemberService = hallOfFameMemberService;
            _discordClient = discordClient as CustomDiscordClient;
            ChannelIdContext = (ulong) channel.DiscordChannelId;

            var guild = ResourceLocator.DiscordClient.GetNullsGuild();
            _channel = _discordClient.GetChannel(guild, (ulong)channel.DiscordChannelId);

            if (_channel == null)
                throw new InvalidOperationException("Discord channel is invalid");

            Messenger.Instance.Register<NewHofEntryMessage>(OnNewHallOfFamemember);

            ClearChannel();
        }


        [CommandHandler(Regex = "plzupdateopening",Debug = true)]
        public async Task UpdateOpening(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            try
            {
                var message = await _channel.GetMessageAsync((ulong)_hofChannel.OpeningMessageId);
                var embed = new DiscordEmbedBuilder(message.Embeds.First());

                embed.Description =
                    "This is hall of fame channel where every election winner will be featured. " +
                    "After election finishes entry will be created here and owner will be able to specify his very own command which will display image featuring winner. You can do this by using command:\n" +
                    $"`{CommandHandlerAttribute.CommandStarter}add command <waifuId> <command> <imgUrl>`\n\n" +
                    "Image can be displayed by anyone with command:\n" +
                    $"`{CommandHandlerAttribute.CommandStarter}hof <command>`";


                await message.ModifyAsync(default, new Optional<DiscordEmbed>(embed.Build()));
            }
            finally
            {
                await args.Message.DeleteAsync();
            }
        }

        [CommandHandler(Regex = @"add command \d{1,7} \w+ .*",HumanReadableCommand = "add command <waifuId> <command> <imgUrl>",HelpText = "Sets command for your winner.")]
        public async Task SetCommand(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var userService = _userService.ObtainLifetimeHandle(executionContext))
            using (var hofMembersService = _hallOfFameMemberService.ObtainLifetimeHandle(executionContext))
            {
                try
                {
                    var para = args.Message.Content.Split(' ');
                    hofMembersService.ConfigureIncludes().WithChain(query =>
                        query.Include(m => m.Contender.Waifu).Include(m => m.Contender.Proposer)).Commit();
                    var member =
                        await hofMembersService.FirstAsync(fameMember => fameMember.Contender.Waifu.MalId == para[2]);

                    if (member != null)
                    {
                        var user = await userService.GetOrCreateUser(args.Author);

                        if (user.Equals(member.Contender.Proposer) || args.Author.IsAuthenticated())
                        {
                            if (para[4].IsLink())
                            {
                                member.CommandName = para[3];
                                member.ImageUrl = para[4];

                                await UpdateInfoEmbed(member);
                            }
                            else
                            {
                                await args.Channel.SendTimedMessage("Provided link doesn't provide art.");
                            }
                        }
                        else
                        {
                            await args.Channel.SendTimedMessage("You didn't propose this contender.");
                        }
                    }
                    else
                    {
                        await args.Channel.SendTimedMessage("No winner with such id.");
                    }
                }
                finally
                {
                    await args.Message.DeleteAsync();
                }
            }
        }

        [CommandHandler(Regex = @"fixembedsplz", Debug = true)]
        public async Task FixEmbeds(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var hofMembersService = _hallOfFameMemberService.ObtainLifetimeHandle(executionContext))
            {
                try
                {
                    hofMembersService.ConfigureIncludes().WithChain(query =>
                        query.Include(m => m.Contender.Waifu).Include(m => m.Contender.Proposer)).Commit();
                    foreach (var hallOfFameMember in hofMembersService.GetAll())
                    {
                        await UpdateInfoEmbed(hallOfFameMember);
                    }
                }
                catch
                {
                    
                }
                finally
                {
                    await args.Message.DeleteAsync();
                }
            }
        }

        [CommandHandler(FallbackCommand = true)]
        public async Task FallbackCommand(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            if (!args.Author.IsMe())
                await args.Message.DeleteAsync();
        }


        private async Task OnNewHallOfFamemember(NewHofEntryMessage newHofEntryMessage)
        {
            var member = newHofEntryMessage.Member;
            var embed = new DiscordEmbedBuilder();

            Election election;

            using (var es = _electionService.ObtainLifetimeHandle(false))
            {
                es.ConfigureIncludes().ExtendChain(query =>
                {
                    return query
                        .Include(e => e.BracketStages).ThenInclude(s => s.Brackets).ThenInclude(b => b.Votes)
                        .ThenInclude(v => v.User)
                        .Include(e => e.BracketStages).ThenInclude(s => s.Brackets).ThenInclude(b => b.Votes)
                        .ThenInclude(v => v.Contender)
                        .Include(e => e.BracketStages).ThenInclude(s => s.Brackets).ThenInclude(b => b.FirstContender)
                        .Include(e => e.BracketStages).ThenInclude(s => s.Brackets).ThenInclude(b => b.SecondContender)
                        .Include(e => e.BracketStages).ThenInclude(s => s.Brackets).ThenInclude(b => b.ThirdContender);

                }).Commit();
                election = await es.FirstAsync(e => e.Name == member.ElectionName);
            }

            //separator
            embed.Description =
                $":trophy::gem: {member.ElectionName} :gem::trophy:";
            embed.Color = DiscordColor.DarkGray;

            var msg = await _channel.SendMessageAsync(null, false, embed);
            member.SeparatorMessageId = (long) msg.Id;



            var bracketsWithWinner = election.BracketStages.SelectMany(stage => stage.Brackets).Where(bracket =>
                bracket.FirstContender.Equals(member.Contender) ||
                bracket.SecondContender.Equals(member.Contender) ||
                (bracket.ThirdContender != null && bracket.ThirdContender.Equals(member.Contender))).ToList();

            var voters = bracketsWithWinner
                .SelectMany(bracket => bracket.Votes)
                .Where(vote => vote.Contender.Equals(member.Contender))
                .Select(vote => vote.User.Name)
                .Distinct(StringComparer.CurrentCultureIgnoreCase).ToList();

            var loyalVoters = new List<string>();
            foreach (var voter in voters)
            {
                if (bracketsWithWinner
                    .All(bracket => bracket.Votes.FirstOrDefault(vote => vote.User.Name.Equals(voter))?
                                        .Contender.Equals(member.Contender) ?? false))
                {
                    loyalVoters.Add(voter);
                }
            }
            var percentages = new List<float>();
            foreach (var bracket in bracketsWithWinner)
            {
                if (bracket.Votes.Any())
                    percentages.Add(bracket.Votes.Count(vote => vote.Contender.Equals(member.Contender)) *
                                           100.0f / bracket.Votes.Count);
                else
                    percentages.Add(0);
            }

            var contenderEmbed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = member.Contender.Proposer.Name,
                    IconUrl = member.Contender.Proposer.AvatarUrl
                },
                Color = DiscordColor.Gold,

                ThumbnailUrl = member.Contender.CustomImageUrl ?? member.Contender.Waifu.ImageUrl,
                Title = member.Contender.Waifu.Name,
                Footer = new DiscordEmbedBuilder.EmbedFooter {Text = $"WaifuId: {member.Contender.Waifu.MalId}"},
            };
            contenderEmbed.WithUrl($"https://myanimelist.net/character/{member.Contender.Waifu.MalId}");

            var tieWins = bracketsWithWinner.SelectMany(bracket => bracket.Votes).Count(vote =>
                vote.Contender.Equals(member.Contender) && vote.User.DiscordId == UserService.MyId);

            var desc = "\u200B\n";
            
            desc +=
                $"**All Votes:** {election.BracketStages.SelectMany(stage => stage.Brackets).SelectMany(bracket => bracket.Votes).Count(vote => vote.Contender.Equals(member.Contender))}\n";
            desc +=
                $"**Final Votes:** {election.BracketStages.Last().Brackets.Last().Votes.Count(vote => vote.Contender.Equals(member.Contender))}\n";
            desc += $"**Loyal Voters:** {(loyalVoters.Any() ? string.Join(", ", loyalVoters) : "None")}\n";
            desc += $"**All Voters:** {(voters.Any() ? string.Join(", ", voters) : "None")}\n";
            desc += $"**Tie Wins:** {tieWins}\n";
            desc += $"**% of votes in brackets** {string.Join(", ",percentages.Select(i => $"{i:N2}%"))}\n";

            contenderEmbed.Description = desc;

            contenderEmbed.AddField(CommandKey, "-");
            msg = await _channel.SendMessageAsync(null, false, contenderEmbed);
            member.ContenderMessageId = (long) msg.Id;
        }

        private async Task UpdateInfoEmbed(HallOfFameMember member)
        {
            var message = await _channel.GetMessageAsync((ulong)member.ContenderMessageId);
            var embed = new DiscordEmbedBuilder(message.Embeds.First());

            embed.Fields.First(field => field.Name.Equals(CommandKey)).Value = member.CommandName ?? "-";

            embed.ThumbnailUrl = member.Contender.CustomImageUrl ?? member.Contender.Waifu.ImageUrl;
            embed.ImageUrl = member.ImageUrl;

            await message.ModifyAsync(default, new Optional<DiscordEmbed>(embed.Build()));
        }

        public async Task<long> OnCreate()
        {
            var embed = new DiscordEmbedBuilder();

            embed.Title = "What's going on?";
            embed.Description =
                "This is hall of fame channel where every election winner will be featured. " +
                "After election finishes entry will be created here and owner will be able to specify his very own command which will display image featuring winner of his choice. You can do this by using command:\n" +
                $"`{CommandHandlerAttribute.CommandStarter}add command <waifuId> <command> <imgUrl>`\n\n" +
                "Image can be displayed by anyone with command:\n" +
                $"`{CommandHandlerAttribute.CommandStarter}hof <command>`";

            embed.Color = DiscordColor.Gray;
            return (long)(await _channel.SendMessageAsync(null, false, embed.Build())).Id;
        }





        public class NewHofEntryMessage
        {
            public HallOfFameMember Member { get; set; }
        }
    }


}
