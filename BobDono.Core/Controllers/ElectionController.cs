using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BobDono.Core.Extensions;
using BobDono.DataAccess.Database;
using BobDono.Interfaces;
using BobDono.Models.Entities;
using DSharpPlus.Entities;

namespace BobDono.Core.Controllers
{
    public class ElectionController
    {     
        private const string CurrentEntriesCount = "Current Entries:";
        private const string ParticipantsCount = "Participants:";

        public Election Election { get; set; }
        private readonly DiscordChannel _channel;
        private readonly IElectionService _electionService;

        private Random _random = new Random();

        public ElectionController(Election election, DiscordChannel channel, IElectionService electionService)
        {
            Election = election;
            _channel = channel;
            _electionService = electionService;
        }


        public async Task ProcessTimePass()
        {
            bool transitioned = false;
            switch (Election.CurrentState)
            {
                case Election.State.Submission:
                    if (DateTime.UtcNow > Election.SubmissionsEndDate)
                    {
                        await TransitionToPendingVoting();
                        transitioned = true;
                    }
                    break;
                case Election.State.PedningVotingStart:
                    if (DateTime.UtcNow > Election.VotingStartDate)
                    {
                        await TransitionToVoting();
                        transitioned = true;
                    }
                    break;
            }

            if (!transitioned && Election.CurrentState == Election.State.Voting)
            {
                var currentStage = Election.BracketStages.Last();
                if (DateTime.UtcNow > currentStage.EndDate)
                {
                    await CloseCurrentStage();
                }
            }

            _electionService.Update(Election);
        }

        public async void Initialize()
        {
            var embed = new DiscordEmbedBuilder();

            embed.Color = DiscordColor.Gold;
            embed.Description = Election.Description;
            embed.Title = $"Election: {Election.Name}";
            embed.Author = new DiscordEmbedBuilder.EmbedAuthor {Name = Election.Author.Name};
            embed.AddField("Submission time:",
                $"{Election.SubmissionsStartDate} - {Election.SubmissionsEndDate} - *({(Election.SubmissionsEndDate - Election.SubmissionsStartDate).Days} days)*");
            embed.AddField("Entrants per person:", Election.EntrantsPerUser.ToString());
            embed.AddField(ParticipantsCount, "0");
            embed.AddField(CurrentEntriesCount, "0");

            var message = await _channel.SendMessageAsync(null, false, embed.Build());

            _electionService.Begin();
            {
                Election.OpeningMessageId = message.Id;
                _electionService.Update(Election);
            }
            _electionService.Finish();


        }

        public async void UpdateOpeningMessage()
        {
            var message = await _channel.GetMessageAsync(Election.OpeningMessageId);

            var embed = new DiscordEmbedBuilder(message.Embeds.First());

            embed.Fields.First(field => field.Name.Equals(ParticipantsCount)).Value = Election.Contenders
                .Select(contender => contender.Proposer.Id).Distinct().Count().ToString();
            embed.Fields.First(field => field.Name.Equals(CurrentEntriesCount)).Value = Election.Contenders.Count.ToString();

            await message.ModifyAsync(default, new Optional<DiscordEmbed>(embed.Build()));
        }


        private async Task TransitionToPendingVoting()
        {
            Election.CurrentState = Election.State.PedningVotingStart;

            var msg = await _channel.SendMessageAsync($"Voting will start at {Election.VotingStartDate}");

            Election.PendingVotingStartMessageId = msg.Id;
        }

        public async Task TransitionToVoting()
        {
            Election.CurrentState = Election.State.Voting;

            //prepare seeds
            var seeds = new List<int>();
            for (int i = 0; i < Election.Contenders.Count; i++)
                seeds.Add(i);
            seeds.Shuffle();
            //add seeds to contenders
            for (int i = 0; i < Election.Contenders.Count; i++)
                Election.Contenders.ElementAt(i).SeedNumber = seeds[i];

            BracketStage stage;
            //create brackets
            if (Election.Contenders.Count % 2 == 1) //if it's odd we are gonna take one contestant out and isert it at random
            {
                var contenders = Election.Contenders.ToList();
                var randomIndex = _random.Next(0, contenders.Count);
                var oddContender = contenders[randomIndex];
                //we pull one out so we have even number
                contenders.Remove(oddContender);
                stage = CreateBracketStage(contenders);

                randomIndex = _random.Next(0, stage.Brackets.Count);
                //and then we insert him to random triple bracket
                stage.Brackets.ElementAt(randomIndex).ThirdContender = oddContender;
            }
            else
            {
                stage = CreateBracketStage(Election.Contenders.ToList());
            }

            Election.BracketStages.Add(stage);

            //proces dates
            Election.StageCount = GetStageCount();
            Election.VotingEndDate = Election.VotingStartDate.AddDays(Election.StageCount);

            //update channel
            var ids = await SendBracketInfo(stage.Brackets);
            Election.BracketMessagesIds = ids;

            try
            {
                var msg = await _channel.GetMessageAsync(Election.PendingVotingStartMessageId);
                await msg.DeleteAsync();
            }
            catch (Exception)
            {
                //debug
            }

        }

        private async Task TransitionToClosed()
        {
            Election.CurrentState = Election.State.Closed;


            //some stats perhaps?
            var embed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.CornflowerBlue,
                Title = "Election has concluded!",
                Description = "Have some stats about it:",
            };

            embed.AddField("Votes:",
                Election.BracketStages.SelectMany(stage => stage.Brackets).Sum(bracket => bracket.Votes.Count)
                    .ToString());
            embed.AddField("Contenders:", Election.Contenders.Count.ToString());
            await _channel.SendMessageAsync(null, false, embed);


            //let's announce the winner
            var winner = Election.BracketStages.Last().Brackets.First().Winner;
            var winnerEmbed = winner.GetEmbedBuilder();
            winnerEmbed.Author = null;
            winnerEmbed.Color = DiscordColor.Gold;
            winnerEmbed.Title = $":trophy: 1st place: {winnerEmbed.Title} :trophy:";

            await _channel.SendMessageAsync(null, false, winnerEmbed);

            //2nd place - second contender from last bracket

            var finalBracket = Election.BracketStages.Last().Brackets.First();
            var secondPlace = finalBracket.FirstContender == winner
                ? finalBracket.SecondContender
                : finalBracket.FirstContender;

            var secondEmbed = secondPlace.GetEmbedBuilder();
            secondEmbed.Author = null;
            secondEmbed.Color = new DiscordColor(192,192,192);
            secondEmbed.Title = $"2nd place: {secondEmbed.Title}";

            await _channel.SendMessageAsync(null, false, secondEmbed);

            //3rd place - contender from brackets that made final bracket

            var halfFinalStage = Election.BracketStages.ElementAt(Election.BracketStages.Count - 2);
            var semiLosers = halfFinalStage.Brackets.Select(bracket => bracket.Loser);
            var winnerOfLosers = FindWinner(semiLosers.ToList());

            var thirdEmbed = winnerOfLosers.GetEmbedBuilder();
            thirdEmbed.Author = null;
            thirdEmbed.Color = DiscordColor.Brown;
            thirdEmbed.Title = $"3rd place: {thirdEmbed.Title}";

            await _channel.SendMessageAsync(null, false, thirdEmbed);

        }

        public async Task CloseCurrentStage()
        {
            //remove old messages
            try
            {
                foreach (var bracketMessageId in Election.BracketMessagesIds)
                {
                    await _channel.DeleteMessageAsync(await _channel.GetMessageAsync(bracketMessageId));
                }
            }
            catch (Exception)
            {
                //we have already deleted them - debug?
            }



            //select winners
            var lastBrackets = Election.BracketStages.Last().Brackets.ToList();
            foreach (var bracket in lastBrackets)
            {
                bracket.Winner = FindWinner(bracket);

                //we are losing one loser out of triple bracket but I don't care
                if (bracket.Winner == bracket.FirstContender)
                    bracket.Loser = bracket.SecondContender;
                else if (bracket.Winner == bracket.SecondContender)
                    bracket.Loser = bracket.FirstContender;
                else if (bracket.Winner == bracket.ThirdContender)
                    bracket.Loser = bracket.FirstContender;
            }


            if (lastBrackets.Count == 1) //this was last bracket so we cannot create new one
            {
                await TransitionToClosed();
            }
            else
            {
                //create new bracket
                var stage = CreateBracketStage(lastBrackets.Select(bracket => bracket.Winner).ToList());

                var ids = await SendBracketInfo(stage.Brackets);
                Election.BracketMessagesIds = ids;

                Election.BracketStages.Add(stage);
            }




        }

        private BracketStage CreateBracketStage(List<WaifuContender> contestants)
        {
            var stage = new BracketStage
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1),
            };

            contestants = contestants.OrderBy(contender => contender.SeedNumber).ToList();

            var brackets = new List<Bracket>();
            for (int i = 0, k = 0; i < contestants.Count - 1; i += 2, k++)
            {
                brackets.Add(new Bracket
                {
                    FirstContender = contestants[i],
                    SecondContender = contestants[i + 1],
                    BracketStage = stage,
                    Number = k
                });
            }

            stage.Brackets = brackets;
            return stage;
        }

        private int GetStageCount()
        {
            var countOfBRackets = 0;
            var count = Election.Contenders.Count;
            //we will make three bracket 
            if (count % 2 == 1)
                count--;

            while ((count = count / 2) != 0)
                countOfBRackets++;

            return countOfBRackets;
        }

        public WaifuContender FindWinner(Bracket bracket)
        {
            var contestants = new List<WaifuContender> { bracket.FirstContender, bracket.SecondContender };
            if (bracket.ThirdContender != null)
                contestants.Add(bracket.ThirdContender);
            return FindWinner(contestants);
        }

        public WaifuContender FindWinner(List<WaifuContender> contestants)
        {
            WaifuContender winner = null;
            //if we don't have remis
            if(contestants.Skip(1).All(contender => contender.Votes.Count != contestants[0].Votes.Count))
            {
                foreach (var waifuContender in contestants)
                {
                    if (winner == null)
                    {
                        winner = waifuContender;
                    }
                    else if (winner.Votes.Count < waifuContender.Votes.Count)
                    {
                        winner = waifuContender;
                    }
                }
            }
            else //we have a problem, frequency of given votes will decide as in measuring voters dedication.
            {
                //not optimal but straightforward
                bool isLowerResmis;
                bool isUpperRemis = false;

                var minVotes = contestants.Min(contender => contender.Votes.Count);
                var maxVotes = contestants.Max(contender => contender.Votes.Count);

                //let's check what are we dealing with
                isLowerResmis = contestants.Count(contender => contender.Votes.Count == minVotes) == 2;
                if (!isLowerResmis)
                {
                    isUpperRemis = contestants.Count(contender => contender.Votes.Count == maxVotes) == 2;
                    //else we have triple remis
                }

                if (isLowerResmis)
                {
                    winner = contestants.First(contender => contender.Votes.Count != minVotes);
                }
                else if(isUpperRemis)
                {
                    var upperRemis = contestants.Where(contender => contender.Votes.Count == maxVotes);
                    winner = GetContenderWithMoreFrequentVotes(upperRemis.First(), upperRemis.Last());
                }
                else
                {
                    var freq = contestants.Select(contender => new {Freq = GetFrequencyForContender(contender), Contender = contender}).ToList();
                    winner = freq.First(arg => arg.Freq.Equals(freq.Max(arg1 => arg1.Freq))).Contender;
                }

                WaifuContender GetContenderWithMoreFrequentVotes(WaifuContender c1, WaifuContender c2)
                {
                    if (GetFrequencyForContender(c1) > GetFrequencyForContender(c2))
                        return c1;
                    return c2;
                }

                float GetFrequencyForContender(WaifuContender contender)
                {
                    var voteCount = contender.Votes.Count;
                    if (voteCount <= 1) //okay... now let's just use old plain random. I don't care. /shrug
                    {
                        return (float)_random.NextDouble();
                    }
                    else
                    {
                        var consecutiveVotePairsCount = (voteCount % 2 == 0 ? voteCount : voteCount - 1)/2; //let's leave last one
                        var pairs = Enumerable.Range(0, consecutiveVotePairsCount)
                            .Select(i => contender.Votes.Skip(i * 2).Take(2)).ToList();
                        return (float)pairs.Sum(votes => (votes.Last().CreateDate - votes.First().CreateDate).TotalHours) /
                                  pairs.Count;

                    }
                }
            }

            return winner;
        }

        private async Task<List<ulong>> SendBracketInfo(ICollection<Bracket> brackets)
        {
            var output = new List<ulong>();
            foreach (var bracket in brackets)
            {
                foreach (var discordEmbed in bracket.GetEmbeds())
                {
                    var msg = await _channel.SendMessageAsync(null, false, discordEmbed);
                    output.Add(msg.Id);
                }
            }
            return output;
        }

    }
}
