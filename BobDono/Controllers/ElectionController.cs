using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BobDono.Contexts;
using BobDono.Core;
using BobDono.Core.Attributes;
using BobDono.Core.BL;
using BobDono.Core.Extensions;
using BobDono.Core.Utils;
using BobDono.DataAccess.Services;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;
using BobDono.Models.Entities;
using BobDono.Utils;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace BobDono.Controllers
{
    public class ElectionController
    {     
        private const string CurrentEntriesCount = "Current Entries:";
        private const string ParticipantsCount = "Participants:";
        private const string TotalVotes = "Total Votes:";
        private const string EntrantsCount = "Entrants per person:";
#if DEBUG
        private const long MentionGroupId = 381412270481342465;
#else
        private const long MentionGroupId = 430059016144551947;
#endif

        public Election Election { get; set; }
        private readonly DiscordChannel _channel;
        private readonly ElectionContext _context;
        private readonly IServiceFactory<IElectionService> _electionService;

        private Random _random = new Random();

        public ElectionController(ElectionContext context, Election election, DiscordChannel channel, IServiceFactory<IElectionService> electionService)
        {
            Election = election;
            _context = context;
            _channel = channel;
            _electionService = electionService;
        }


        public async Task ProcessTimePass(IUserService userService, IElectionService electionService)
        {
            Election = await electionService.GetElection(Election.Id);

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
                    await CloseCurrentStage(userService);
                }
            }
        }

        public async void Initialize()
        {
            var embed = new DiscordEmbedBuilder();

            embed.Title = "What's going on?";
            embed.Description =
                "Welcome to so called election! (aka. Waifu War) I'll try to briefly describe how it works :)\n\n" +
                $"Elections have 2 stages:\n\n**submission** stage is where everyone is allowed to add certain amount of contenders, we are using MAL ids to make it work. In order to add conteder use\n`{CommandHandlerAttribute.CommandStarter}add contender <id> [thumbnailOverride=none] [featureImage]`\n\n" +
                $"**voting** stage where all contenders are arranged into brackets, you can vote once in every bracket using command:\n`{CommandHandlerAttribute.CommandStarter}vote <bracketNumber> <contenderNumber>`\n\n" +
                "In case of there being odd number of contestants triple bracket will be created. In case of remis there are some *algorithms* in place to resolve them... go to github if you are curious." +
                " Each stage lasts one day. When the final stage ends, first 3 places will be announced and whole election will transition into closed state (I won't be listening for any more messages)" +
                " Please be aware that this channel is entirely up to my disposition, that means *I'll remove your messages* so things stay organised here. Due to that muting this channel is advised. Have fun!";

            embed.Color = DiscordColor.Gray;
            var msg = await _channel.SendMessageAsync(null, false, embed.Build());
            await msg.PinAsync();

            embed = new DiscordEmbedBuilder();

            embed.Color = DiscordColor.CornflowerBlue;
            embed.Description = Election.Description;
            embed.Title = $":book: Election: {Election.Name}";
            embed.Author = new DiscordEmbedBuilder.EmbedAuthor {Name = Election.Author.Name};
            embed.AddField("Submission time:",
                $"{Election.SubmissionsStartDate} - {Election.SubmissionsEndDate} (UTC) - *({(Election.SubmissionsEndDate - Election.SubmissionsStartDate).Days} days)*");
            embed.AddField(EntrantsCount, Election.EntrantsPerUser.ToString());
            embed.AddField(ParticipantsCount, "0");
            embed.AddField(CurrentEntriesCount, "0");
            embed.AddField(TotalVotes, "0");

            await _channel.SendMessageAsync($"<@&{MentionGroupId}>");
            var message = await _channel.SendMessageAsync(null, false, embed.Build());
            await message.PinAsync();
            using (var electionService = _electionService.ObtainLifetimeHandle())
            {
                Election = await electionService.GetElection(Election.Id);
                Election.OpeningMessageId = message.Id;
            }
        }

        public async void UpdateOpeningMessage()
        {
            var message = await _channel.GetMessageAsync(Election.OpeningMessageId);

            var embed = new DiscordEmbedBuilder(message.Embeds.First());


            embed.Fields.First(field => field.Name.Equals(EntrantsCount)).Value = Election.EntrantsPerUser.ToString();  
            
            embed.Fields.First(field => field.Name.Equals(ParticipantsCount)).Value = Election.Contenders
                .Select(contender => contender.Proposer.Id).Distinct().Count().ToString();
            embed.Fields.First(field => field.Name.Equals(CurrentEntriesCount)).Value = Election.Contenders.Count.ToString();
            embed.Fields.First(field => field.Name.Equals(TotalVotes)).Value = Election.BracketStages
                .SelectMany(stage => stage.Brackets).Select(bracket => bracket.Votes.Count).Sum().ToString();

            await message.ModifyAsync(default, new Optional<DiscordEmbed>(embed.Build()));
        }


        private async Task TransitionToPendingVoting()
        {
            Election.CurrentState = Election.State.PedningVotingStart;

            var msg = await _channel.SendMessageAsync($"<@&{MentionGroupId}> Voting will start at {Election.VotingStartDate}");

            Election.PendingVotingStartMessageId = msg.Id;
        }

        public async Task TransitionToVoting()
        {


            if (Election.Contenders.Count > 1)
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

                stage = CreateBracketStage(Election.Contenders.ToList());

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
            else
            {
                Election.CurrentState = Election.State.Closed;

                await _channel.SendMessageAsync(
                    "There was not enough contenders to create brackets. Election is now closed.");

                _context.DeregisterTimer();
            }
            

        }

        private async Task TransitionToClosed()
        {
            Election.CurrentState = Election.State.Closed;


            //some stats perhaps?
            //var embed = new DiscordEmbedBuilder()
            //{
            //    Color = DiscordColor.CornflowerBlue,
            //    Title = "Election has concluded!",
            //    Description = "Have some stats about it:",
            //};

            //embed.AddField("Votes:",
            //    Election.BracketStages.SelectMany(stage => stage.Brackets).Sum(bracket => bracket.Votes.Count)
            //        .ToString());
            //embed.AddField("Contenders:", Election.Contenders.Count.ToString());
            //await _channel.SendMessageAsync(null, false, embed);


            //let's announce the winner
            var finalBracket = Election.BracketStages.Last().Brackets.First();
            var winner = finalBracket.Winner;
            var winnerEmbed = winner.GetEmbedBuilder();
            winnerEmbed.Author = null;
            winnerEmbed.Color = DiscordColor.Gold;
            winnerEmbed.Title = $":trophy: 1st place: {winnerEmbed.Title} :trophy:";

            winnerEmbed.AddField("Final votes:", FormatVotes(winner, finalBracket.BracketStage));
                

            await _channel.SendMessageAsync(null, false, winnerEmbed);

            DiscordEmbedBuilder secondEmbed;
            DiscordEmbedBuilder thirdEmbed;

            if (finalBracket.ThirdContender == null)
            {
                //2nd place - second contender from last bracket
                var secondPlace = finalBracket.Loser;

                secondEmbed = secondPlace.GetEmbedBuilder();

                secondEmbed.AddField("Final votes:", FormatVotes(secondPlace, finalBracket.BracketStage));

                //3rd place - contender from brackets that made final brackets
                var halfFinalStage = Election.BracketStages.ElementAt(Election.BracketStages.Count - 2);
                var semiLosers = halfFinalStage.Brackets.Select(bracket => bracket.Loser);
                var winnerOfLosers = FindWinner(semiLosers.ToList(),halfFinalStage);

                thirdEmbed = winnerOfLosers.GetEmbedBuilder();

                thirdEmbed.AddField("Final votes:", FormatVotes(winnerOfLosers, halfFinalStage));
            }
            else
            {
                var notWinners =
                    new[] {finalBracket.FirstContender, finalBracket.SecondContender, finalBracket.ThirdContender}
                        .Where(contender => !contender.Equals(finalBracket.Winner)).ToList();
                var secondPlace = FindWinner(notWinners,finalBracket.BracketStage);

                secondEmbed = secondPlace.GetEmbedBuilder();
                secondEmbed.AddField("Final votes:", FormatVotes(secondPlace, finalBracket.BracketStage));

                var winnerOfLosers = notWinners.First(c => !c.Equals(secondPlace) && !c.Equals(finalBracket.Winner));
                thirdEmbed = winnerOfLosers.GetEmbedBuilder();
                thirdEmbed.AddField("Final votes:", FormatVotes(winnerOfLosers, finalBracket.BracketStage));
            }

            secondEmbed.Author = null;
            secondEmbed.Color = new DiscordColor(192, 192, 192);
            secondEmbed.Title = $"2nd place: {secondEmbed.Title}";

            await _channel.SendMessageAsync(null, false, secondEmbed);

            thirdEmbed.Author = null;
            thirdEmbed.Color = DiscordColor.Brown;
            thirdEmbed.Title = $"3rd place: {thirdEmbed.Title}";

            await _channel.SendMessageAsync(null, false, thirdEmbed);

            _context.DeregisterTimer();

            string FormatVotes(WaifuContender contender, BracketStage stage)
            {
                var votes = contender.Votes.Where(vote => vote.Bracket.BracketStage.Equals(stage)).ToList();
                return
                    votes.Any()
                        ? $"(**{votes.Count} votes**: {string.Join(", ", votes.Select(vote => vote.User.Name))})"
                        : $"(**{votes.Count} votes**)";

            }
        }

        public async Task CloseCurrentStage(IUserService userService)
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
                if (bracket.Winner.Equals(bracket.FirstContender))
                    bracket.Loser = bracket.SecondContender;
                else if (bracket.Winner.Equals(bracket.SecondContender))
                    bracket.Loser = bracket.FirstContender;
                else if (bracket.Winner.Equals(bracket.ThirdContender))
                    bracket.Loser = bracket.FirstContender;

                if (bracket.Winner.Votes.Count(FilterVoteForCurrentBracket) == bracket.Loser.Votes.Count(FilterVoteForCurrentBracket))
                {
                    bracket.Winner.Votes.Add(new Vote
                    {
                        Bracket = bracket,
                        Contender = bracket.Winner,
                        CreateDate = DateTime.UtcNow,
                        User = await userService.GetBobUser()
                    });
                }

                bool FilterVoteForCurrentBracket(Vote v) => v.Bracket.BracketStage.Equals(bracket.BracketStage);
            }

            await _channel.SendMessageAsync($"<@&{MentionGroupId}>");
            if (lastBrackets.Count == 1) //this was last bracket so we cannot create new one
            {
                await TransitionToClosed();
            }
            else
            {
                var lastStage = Election.BracketStages.Last();
                var first = true;
                //announce winners
                foreach (var bracket in lastStage.Brackets)
                {
                    var embed = new DiscordEmbedBuilder();
                    if (first)
                    {
                        embed.Title = $"Results of stage #{lastStage.Number}";
                        first = false;
                    }

                    embed.Color = DiscordColor.Gray;

                    var content =
                        $"**Winner**: {Format(bracket.Winner)}\n" +
                        $"**Loser**: {Format(bracket.Loser)}";

                    if (bracket.ThirdContender != null)
                    {
                        var loser =
                            new[] {bracket.FirstContender, bracket.SecondContender, bracket.ThirdContender}.First(
                                contender => contender.Id != bracket.Winner.Id && contender.Id != bracket.Loser.Id);
                        content += $"\n**Loser**: {Format(loser)}";
                    }

                    embed.AddField($"Bracket #{bracket.Number}", content);

                    string Format(WaifuContender con)
                    {
                        var votes = con.Votes.Where(vote => vote.Bracket.BracketStage.Equals(lastStage)).ToList();
                        return
                            votes.Any()
                                ? $"{con.Waifu.Name} (**{votes.Count} votes**: {string.Join(", ", votes.Select(vote => vote.User.Name))})"
                                : $"{con.Waifu.Name} (**{votes.Count} votes**)";
                    }


                    await _channel.SendMessageAsync(null, false, embed);
                }

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
                Number = Election.BracketStages.Count + 1,
            };

            bool odd = false;
            int randomIndex = 0;
            WaifuContender oddContender = null;

            if (contestants.Count % 2 == 1) //if it's odd we are gonna take one contestant out and isert it at random
            {
                randomIndex = _random.Next(0, contestants.Count);
                oddContender = contestants[randomIndex];
                //we pull one out so we have even number
                contestants.Remove(oddContender);

                odd = true;
            }
         
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

            if (odd)
            {
                randomIndex = _random.Next(0, brackets.Count);
                //and then we insert him to random triple bracket
                brackets.ElementAt(randomIndex).ThirdContender = oddContender;
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
            return FindWinner(contestants, bracket.BracketStage);
        }

        public WaifuContender FindWinner(List<WaifuContender> contestants, BracketStage bracket)
        {
            WaifuContender winner = null;
            //if we don't have remis
            var group = contestants.GroupBy(contender => contender.Votes.Count(FilterVoteForCurrentBracket));
            var remis = group.Any(grouping => grouping.Count() > 1);
            if (!remis)
            {
                foreach (var waifuContender in contestants)
                {
                    if (winner == null)
                    {
                        winner = waifuContender;
                    }
                    else if (winner.Votes.Count(FilterVoteForCurrentBracket) < waifuContender.Votes.Count(FilterVoteForCurrentBracket))
                    {
                        winner = waifuContender;
                    }
                }
            }
            else //we have a problem, frequency of given votes will decide as in measuring voters dedication.
            {
                //not optimal but straightforward
                bool isLowerResmis = false;
                bool isUpperRemis = false;

                var minVotes = contestants.Min(contender => contender.Votes.Count(FilterVoteForCurrentBracket));
                var maxVotes = contestants.Max(contender => contender.Votes.Count(FilterVoteForCurrentBracket));

                if (contestants.Count > 2)
                {
                    //let's check what are we dealing with
                    isLowerResmis = contestants.Count(contender =>
                                        contender.Votes.Count(FilterVoteForCurrentBracket) == minVotes) == 2;
                    if (!isLowerResmis)
                    {
                        isUpperRemis = contestants.Count(contender =>
                                           contender.Votes.Count(FilterVoteForCurrentBracket) == maxVotes) == 2;
                        //else we have triple remis
                    }
                }

                if (isLowerResmis && minVotes > 0)
                {
                    winner = contestants.First(contender => contender.Votes.Count(FilterVoteForCurrentBracket) != minVotes);
                }
                else if (isUpperRemis && minVotes > 0)
                {
                    var upperRemis = contestants.Where(contender => contender.Votes.Count(FilterVoteForCurrentBracket) == maxVotes);
                    winner = GetContenderWithMoreFrequentVotes(upperRemis.First(), upperRemis.Last());
                }
                else
                {
                    var freq = contestants.Select(contender =>
                        new {Freq = GetFrequencyForContender(contender), Contender = contender}).ToList();
                    winner = freq.First(arg => arg.Freq.Equals(freq.Min(arg1 => arg1.Freq))).Contender;
                }

                WaifuContender GetContenderWithMoreFrequentVotes(WaifuContender c1, WaifuContender c2)
                {
                    if (GetFrequencyForContender(c1) > GetFrequencyForContender(c2))
                        return c1;
                    return c2;
                }

                float GetFrequencyForContender(WaifuContender contender)
                {
                    return (float)_random.NextDouble();
                    //var voteCount = contender.Votes.Count(FilterVoteForCurrentBracket);
                    //if (voteCount <= 1) //okay... now let's just use old plain random. I don't care. /shrug
                    //{
                        
                    //}
                    //else
                    //{
                    //    var consecutiveVotePairsCount =
                    //        (voteCount % 2 == 0 ? voteCount : voteCount - 1) / 2; //let's leave last one
                    //    var pairs = Enumerable.Range(0, consecutiveVotePairsCount)
                    //        .Select(i => contender.Votes.Where(FilterVoteForCurrentBracket).Skip(i * 2).Take(2)).ToList();
                    //    return (float) pairs.Sum(votes =>
                    //               Math.Abs((votes.Last().CreateDate - votes.First().CreateDate).TotalHours)) /
                    //           pairs.Count;
                    //}
                }


            }

            return winner;


            bool FilterVoteForCurrentBracket(Vote v) => v.Bracket.BracketStage.Equals(bracket);
        }

        private async Task<List<ulong>> SendBracketInfo(ICollection<Bracket> brackets)
        {
            var output = new List<ulong>();
            foreach (var bracket in brackets)
            {
                var thumbs = new List<byte[]>();
                var httpClient = new HttpClient();
                thumbs.Add(await ObtainImage(bracket.FirstContender));
                thumbs.Add(await ObtainImage(bracket.SecondContender));
                if (bracket.ThirdContender != null)
                    thumbs.Add(await ObtainImage(bracket.ThirdContender));
                httpClient.Dispose();

                using (var image = await Task.Run(() => BracketImageGenerator.Generate(thumbs)))
                {
                    var reactions = new List<string> {":one:", ":two:"};
                    image.Seek(0, SeekOrigin.Begin);
                    var msgHeader = await _channel.SendMessageAsync(null, false, new DiscordEmbedBuilder
                    {
                        Title = $"Bracket #{bracket.Number}",
                        Color = DiscordColor.Brown,
                    });
                    var imgMessage = await _channel.SendFileAsync(image, $"Bracket {bracket.Number}.png");
                    var footerString = $"**1**: {bracket.FirstContender.Waifu.Name}\n";
                    footerString += $"**2**: {bracket.SecondContender.Waifu.Name}";
                    if (bracket.ThirdContender != null)
                    {
                        footerString += $"\n**3**: {bracket.ThirdContender.Waifu.Name}";
                        reactions.Add(":three:");
                    }

                    var msgFooter = await _channel.SendMessageAsync(null, false, new DiscordEmbedBuilder
                    {
                        Description = footerString,
                    });
                    foreach (var reaction in reactions)
                        await msgFooter.CreateReactionAsync(DiscordEmoji.FromName(ResourceLocator.DiscordClient,
                            reaction));

                    output.Add(msgHeader.Id);
                    output.Add(imgMessage.Id);
                    output.Add(msgFooter.Id);
                }

                async Task<byte[]> ObtainImage(WaifuContender contender)
                {
                    try
                    {
                        if (contender.CustomImageUrl != null && !await contender.CustomImageUrl.IsValidImageLink())
                            throw new Exception();
                        return await httpClient.GetByteArrayAsync(contender.CustomImageUrl ?? contender.Waifu.ImageUrl);
                    }
                    catch (Exception e)
                    {
                        ResourceLocator.ExceptionHandler.Handle(e, contender.CustomImageUrl ?? contender.Waifu.ImageUrl);
                        return await httpClient.GetByteArrayAsync(contender.Waifu.ImageUrl);
                    }                   
                }
            }
            return output;
        }

    }
}
