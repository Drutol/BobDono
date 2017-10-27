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


        public void ProcessTimePass()
        {
            bool transitioned = false;
            switch (Election.CurrentState)
            {
                case Election.State.Submission:
                    if (DateTime.UtcNow > Election.SubmissionsEndDate)
                    {
                        TransitionToPendingVoting();
                        transitioned = true;
                    }
                    break;
                case Election.State.PedningVotingStart:
                    if (DateTime.UtcNow > Election.VotingStartDate)
                    {
                        TransitionToVoting();
                        transitioned = true;
                    }
                    break;
                case Election.State.Voting:
                    if (DateTime.UtcNow > Election.VotingEndDate)
                    {
                        TransitionToClosed();
                        transitioned = true;
                    }
                    break;
            }

            if (!transitioned && Election.CurrentState == Election.State.Voting)
            {
                var currentStage = Election.BracketStages.Last();
                if (DateTime.UtcNow > currentStage.EndDate)
                {
                    CloseCurrentBracket();
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
            embed.Author = new DiscordEmbedBuilder.EmbedAuthor { Name = Election.Author.Name };
            embed.AddField("Submission time:",
                $"{Election.SubmissionsStartDate} - {Election.SubmissionsEndDate} - *({(Election.SubmissionsEndDate - Election.SubmissionsStartDate).Days} days)*");
            embed.AddField("Entrants per person:", Election.EntrantsPerUser.ToString());
            embed.AddField(ParticipantsCount, "0");
            embed.AddField(CurrentEntriesCount, "0");

            var message = await _channel.SendMessageAsync(null, false, embed.Build());

            using (var update = await _electionService.ObtainElectionUpdate(Election.Id))
            {
                update.Entity.OpeningMessageId = message.Id;
            }
      
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

            var msg = await _channel.GetMessageAsync(Election.PendingVotingStartMessageId);
            await msg.DeleteAsync();
        }

        private async Task TransitionToClosed()
        {
            Election.CurrentState = Election.State.Closed;
        }

        private void CloseCurrentBracket()
        {
            
        }

        private BracketStage CreateBracketStage(List<WaifuContender> contestants)
        {
            var stage = new BracketStage
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1)
            };

            contestants = contestants.OrderBy(contender => contender.SeedNumber).ToList();

            var brackets = new List<Bracket>();
            for (int i = 0; i < contestants.Count - 1; i += 2)
            {
                brackets.Add(new Bracket
                {
                    FirstContender = contestants[i],
                    SecondContender = contestants[i + 1],
                    BracketStage = stage
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

        private async Task<List<ulong>> SendBracketInfo(ICollection<Bracket> brackets)
        {
            var output = new List<ulong>();
            foreach (var bracket in brackets)
            {
                foreach (var discordEmbed in bracket.GetEmbed())
                {
                    var msg = await _channel.SendMessageAsync(null, false, discordEmbed);
                    output.Add(msg.Id);
                }
            }
            return output;
        }

    }
}
