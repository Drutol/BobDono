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
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Controllers
{
    public class MatchupController
    {
        private readonly DiscordChannel _channel;
        private readonly IServiceFactory<IMatchupService> _matchupService;
        private readonly DiscordClient _discordClient;

        public Matchup Matchup { get; set; }

        public MatchupController(Matchup matchup, DiscordChannel channel, IServiceFactory<IMatchupService> matchupService, DiscordClient discordClient)
        {
            Matchup = matchup;

            _channel = channel;
            _matchupService = matchupService;
            _discordClient = discordClient;
        }

        public async Task ProcessTimePass(ICommandExecutionContext context)
        {
            using (var matchupService = _matchupService.ObtainLifetimeHandle(context))
            {
                matchupService.ConfigureIncludes()
                    .WithChain(query =>
                    {
                        return query.Include(m => m.MatchupPairs)
                            .ThenInclude(p => p.First)
                            .Include(m => m.MatchupPairs)
                            .ThenInclude(p => p.Second)
                            .Include(m => m.Participants).ThenInclude(p => p.User);
                    }).Commit();
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
                        await NotifySlackers();
                        if (DateTime.UtcNow > Matchup.ChallengesEndDate)
                        {
                            await TransitionToClosed();
                        }
                        break;
                }
            }
        }

        private async Task NotifySlackers()
        {
            if ((Matchup.ChallengesEndDate - DateTime.UtcNow).Hours == 24)
            {
                foreach (var pair in Matchup.MatchupPairs)
                {
                    if (pair.FirstParticipantsChallengeCompletionDate == default)
                    {
                        await NotifyUser(pair.First.DiscordId);
                    }

                    if (pair.SecondParticipantsChallengeCompletionDate == default)
                    {
                        await NotifyUser(pair.Second.DiscordId);
                    }
                }
            }

            async Task NotifyUser(ulong id)
            {
                var channel =
                    await _discordClient.CreateDmAsync(await _discordClient.GetNullsGuild()
                        .GetMemberAsync(id));
                await channel.SendMessageAsync(
                    $"Hey! Be sure to mark your challenge as completed in <#{Matchup.DiscordChannelId}>!");
            }
        }

        public async Task TransitionToClosed()
        {
            Matchup.CurrentState = Matchup.State.Finished;

            var shameList = new List<string>();

            foreach (var pair in Matchup.MatchupPairs)
            {
                if (pair.FirstParticipantsChallengeCompletionDate == default && 
                    pair.FirstParticipantsChallenge != null)
                {
                    shameList.Add($"**{pair.First.Name}**\n{pair.FirstParticipantsChallenge}\n\n");
                }

                if (pair.SecondParticipantsChallengeCompletionDate == default &&
                    pair.SecondParticipantsChallenge != null)
                {
                    shameList.Add($"**{pair.Second.Name}**\n{pair.SecondParticipantsChallenge}\n\n");
                }
            }
            if (shameList.Any())
            {
                await _channel.SendMessageAsync(
                    $"It looks like some people didn't complete their challenges...\n:bell::bell::bell:\n\n{string.Concat(shameList)}".Trim());
            }
            else
            {
                await _channel.SendMessageAsync("That would be it... everybody completed their challenges! Splendid!");
            }

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

            if (pair.FirstParticipantsChallengeCompletionDate != default)
            {
                if(!embed.Fields.First().Name.Contains(":white_check_mark:"))
                    embed.Fields.First().Name += " :white_check_mark:";

                if (pair.FirstNotes != null)
                    embed.Fields.First().Value =
                        $"{pair.FirstParticipantsChallenge}\n\n**{pair.First.Name}'s note:**\n{pair.FirstNotes}";
                else
                    embed.Fields.First().Value = pair.FirstParticipantsChallenge;
            }
            else
            {
                embed.Fields.First().Value = pair.FirstParticipantsChallenge ?? "N/A";
            }

            if (pair.SecondParticipantsChallengeCompletionDate != default)
            {
                if (!embed.Fields.Last().Name.Contains(":white_check_mark:"))
                    embed.Fields.Last().Name += " :white_check_mark:";

                if (pair.SecondNotes != null)
                    embed.Fields.Last().Value =
                        $"{pair.SecondParticipantsChallenge}\n\n**{pair.Second.Name}'s note:**\n{pair.SecondNotes}";
                else
                    embed.Fields.Last().Value = pair.SecondParticipantsChallenge;
            }
            else
            {
                embed.Fields.Last().Value = pair.SecondParticipantsChallenge ?? "N/A";
            }

            await message.ModifyAsync(default, embed);        
        }
    }
}
