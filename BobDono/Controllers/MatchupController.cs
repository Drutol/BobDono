using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BobDono.Contexts;
using BobDono.Core.Extensions;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;
using BobDono.Models.Entities;
using DSharpPlus.Entities;

namespace BobDono.Controllers
{
    public class MatchupController
    {
        private readonly DiscordChannel _channel;
        private readonly IMatchupService _matchupService;

        public Matchup Matchup { get; set; }

        public MatchupController(Matchup matchup, DiscordChannel channel, IMatchupService  matchupService)
        {
            Matchup = matchup;

            _channel = channel;
            _matchupService = matchupService;
        }

        public async Task ProcessTimePass(ICommandExecutionContext context)
        {
            using (var matchupService = _matchupService.ObtainLifetimeHandle(context))
            {
                Matchup = await matchupService.GetMatchup(Matchup.Id);

                switch (Matchup.CurrentState)
                {
                    case Matchup.State.Submissions:
                        if (DateTime.UtcNow > Matchup.SignupsEndDate)
                        {
                            await TransitionToRunning();
                        }
                        break;
                    case Matchup.State.Running:
                        if (DateTime.UtcNow > Matchup.ChallengesEndDate)
                        {
                            await TransitionToClosed();
                        }
                        break;
                }
            }
        }

        public async Task TransitionToClosed()
        {
            Matchup.CurrentState = Matchup.State.Finished;

            await _channel.SendMessageAsync("That would be it... hope you had fun!");
        }

        public async Task TransitionToRunning()
        {
            var participants = Matchup.Participants.ToList();

            var count = participants.Count;
            //we take even number of people
            count -= count % 2 == 1 ?  1 : 0;

            participants.Shuffle();
            var pairs = new List<MatchupPair>();

            for (int i = 0, k = 1; i < count - 1; i += 2, k++)
            {
                pairs.Add(new MatchupPair
                {
                    First = participants[i].User,
                    Second = participants[i + 1].User,
                    Matchup = Matchup,
                    Number = k,
                });
            }

            Matchup.CurrentState = Matchup.State.Running;

            foreach (var matchupPair in pairs)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Chartreuse,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        IconUrl = matchupPair.First.AvatarUrl,
                        Name = matchupPair.First.Name
                    },
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        IconUrl = matchupPair.Second.AvatarUrl,
                        Text = matchupPair.Second.Name
                    }
                };

                embed.AddField($"{matchupPair.First.Name}'s assigned challenge:", "N/A");
                embed.AddField($"{matchupPair.Second.Name}'s assigned challenge:", "N/A");

                matchupPair.DiscordMessageId = (long)(await _channel.SendMessageAsync(null, false, embed)).Id;
            }

            Matchup.MatchupPairs = pairs;
        }

        public async Task UpdateOpeningMessage()
        {
            var message = await _channel.GetMessageAsync((ulong)Matchup.OpeningMessageId);

            var embed = new DiscordEmbedBuilder(message.Embeds.First());

            embed.Fields.First(field => field.Name.Equals(MatchupContext.ParticipantsKey)).Value = Matchup.Participants.Any() ?
                string.Join(", ", Matchup.Participants.Select(matchup => matchup.User.Name)) : "-";

            await message.ModifyAsync(default, embed);
        }

        public async Task UpdatePairMessage(MatchupPair pair)
        {
            var message = await _channel.GetMessageAsync((ulong)pair.DiscordMessageId);

            var embed = new DiscordEmbedBuilder(message.Embeds.First());

            embed.Fields.First().Value = pair.FirstParticipantsChallenge ?? "N/A";
            embed.Fields.Last().Value = pair.SecondParticipantsChallenge ?? "N/A";

            await message.ModifyAsync(default, embed);
        }
    }
}
