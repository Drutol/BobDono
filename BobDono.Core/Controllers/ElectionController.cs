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

            var stage = new BracketStage
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1)
            };

            //prepare seeds
            var seeds = new List<int>();
            for (int i = 0; i < Election.Contenders.Count; i++)
                seeds.Add(i);
            seeds.Shuffle();
            //add seeds to contenders
            for (int i = 0; i < Election.Contenders.Count; i++)
                Election.Contenders.ElementAt(i).SeedNumber = seeds[i];

            var contenders = Election.Contenders.ToList();
            contenders = contenders.OrderBy(contender => contender.SeedNumber).ToList();

            var brackets = new List<Bracket>();
            for (int i = 0; i < contenders.Count-1 ; i+=2)
            {
                brackets.Add(new Bracket
                {
                    FirstContender = contenders[i],
                    SecondContender = contenders[i+1],   
                    BracketStage = stage
                });
            }

            //we have one lucky one that passes to next stage
            if (contenders.Count % 2 == 1)
            {
                brackets.Add(new Bracket
                {
                    FirstContender = contenders.Last(),
                    BracketStage = stage
                });
            }

            stage.Brackets = brackets;

            Election.BracketStages.Add(stage);

            foreach (var bracket in brackets)
            {
                foreach (var discordEmbed in bracket.GetEmbed())
                {
                    await _channel.SendMessageAsync(null, false, discordEmbed);
                }
            }

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
    }
}
