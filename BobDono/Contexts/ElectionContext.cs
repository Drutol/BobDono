using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BobDono.Controllers;
using BobDono.Core;
using BobDono.Core.Attributes;
using BobDono.Core.BL;
using BobDono.Core.Extensions;
using BobDono.Core.Utils;
using BobDono.DataAccess.Database;
using BobDono.DataAccess.Services;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;
using BobDono.Models.Entities;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Contexts
{
    [Module(IsChannelContextual = true,Name = "Election(Channel)",Description = "Offers commands to participate in election in current channel. These commands only work in context of appropriate channel.")]
    public class ElectionContext : ContextModuleBase
    {
        private Election _election;
        private readonly CustomDiscordClient _discordClient;
        private DiscordChannel _channel;

        public override DiscordChannel Channel => _channel;

        private readonly ElectionController _controller;

        private readonly IServiceFactory<IWaifuService> _waifuService;
        private readonly IServiceFactory<IElectionService> _electionService;
        private readonly IServiceFactory<IUserService> _userService;
        private readonly IServiceFactory<IContenderService> _contenderService;
        private readonly IServiceFactory<IHallOfFameMemberService> _hallOfFameMemberService;

        private readonly IExceptionHandler _exceptionHandler;

        private TimerService.TimerRegistration _timerRegistration;

        public ElectionContext(Election election, DiscordClient discordClient,
            IServiceFactory<IWaifuService> waifuService, IServiceFactory<IElectionService> electionService,
            IServiceFactory<IUserService> userService, IServiceFactory<IContenderService> contenderService,
            IExceptionHandler exceptionHandler, IServiceFactory<IHallOfFameMemberService> hallOfFameMemberService) :
            base(election.DiscordChannelId)
        {
            _waifuService = waifuService;
            _electionService = electionService;
            _userService = userService;
            _contenderService = contenderService;
            _exceptionHandler = exceptionHandler;
            _hallOfFameMemberService = hallOfFameMemberService;

            _election = election;
            _discordClient = discordClient as CustomDiscordClient;

            var guild = _discordClient.GetNullsGuild();
            _channel = _discordClient.GetChannel(guild, election.DiscordChannelId);

            if (_channel == null)
                throw new InvalidOperationException("Discord channel is invalid");

            ChannelIdContext = election.DiscordChannelId;

            _controller = new ElectionController(this, _election, _channel, _electionService);

            _timerRegistration = new TimerService.TimerRegistration
            {
                Interval = TimeSpan.FromHours(1),
                Task = OnHourPassed
            }.FireOnNextFullHour();

            TimerService.Instance.Register(_timerRegistration);

            ClearChannel();
        }

        #region Commands

        private SemaphoreSlim _addContenderSemaphore = new SemaphoreSlim(1);
        private SemaphoreSlim _voteSemaphore = new SemaphoreSlim(1);

        [CommandHandler(Regex = @"add contender \d+\s?(.*)?",
            HumanReadableCommand = "add contender <malId> [imageOverride=none] [featureImage]",
            HelpText =
                "Adds contender to election if election is in submission period. " +
                "Additionaly default image can be overriden in case of default one being insufficient " +
                "to capture the glory of your proposed character.")]
        public async Task AddContender(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            await _addContenderSemaphore.WaitAsync();
            try
            {
                using (var userService = _userService.ObtainLifetimeHandle(executionContext))
                using (var contenderService = _contenderService.ObtainLifetimeHandle(executionContext))
                using (var electionService = _electionService.ObtainLifetimeHandle(executionContext))
                using (var waifuService = _waifuService.ObtainLifetimeHandle(executionContext))
                using (var hallOfFameMemberService = _hallOfFameMemberService.ObtainLifetimeHandle(executionContext))
                {
                    _election = await electionService.GetElection(_election.Id);
                    if (_election.CurrentState == Election.State.Submission)
                    {
                        var user = await userService.GetOrCreateUser(args.Author);
                        var count = _election.Contenders?.Count(c => c.Proposer.Id == user.Id);

                        var arguments = args.Message.Content.Split(' ');

                        var malId = arguments[2];
                        hallOfFameMemberService.ConfigureIncludes().WithChain(query => query.Include(member => member.Contender.Waifu)).Commit();
                        
                        //check if user didn't create more then he should be able to
                        if (count >= _election.EntrantsPerUser)
                        {
                            await args.Channel.SendTimedMessage($"You have already added {count} contestants.");
                        }
                        else if(await hallOfFameMemberService.FirstAsync(member => member.Contender.Waifu.MalId == malId) != null)
                        {
                            await args.Channel.SendTimedMessage(
                                "This contender has already won an election.");
                        }
                        else if (_election.Contenders?.Any(contender => contender.Waifu.MalId == malId) ?? false)
                        {
                            await args.Channel.SendTimedMessage(
                                "This contender has been already proposed by someone else.");
                        }
                        else
                        {
                            await args.Channel.TriggerTypingAsync();

                            string thumb = null;
                            string feature = null;

                            if (arguments.Length >= 4)
                            {
                                if (arguments[3] != "none" && arguments[3].IsLink())
                                {
                                    if(await arguments[3].IsValidImageLink())
                                        thumb = arguments[3];
                                    else
                                    {
                                        await args.Channel.SendTimedMessage(
                                            "Your thumbnail image is invalid. Either it's not an image or it's larger than 5Mb.");
                                        return;
                                    }
                                }
                            }

                            if (arguments.Length == 5)
                            {
                                if (arguments[4].IsLink())
                                {

                                    if (await arguments[4].IsValidImageLink())
                                        feature = arguments[4];
                                    else
                                    {
                                        await args.Channel.SendTimedMessage(
                                            "Your feature image is invalid. Either it's not an image or it's larger than 5Mb.");
                                        return;
                                    }
                                }
                            }
                            else if (_election.FeatureImageRequired)
                            {
                                await args.Channel.SendTimedMessage(
                                    "This election requires you to provide feature image for your contender... put some heart into it...\nThis message will self destruct in 10 seconds so copy your previous command.",
                                    TimeSpan.FromSeconds(10));
                                return;
                            }

                            var waifu = await waifuService.GetOrCreateWaifu(arguments[2]);
                            var contender = contenderService.CreateContender(user, waifu, _election);
                            contender.FeatureImage = feature;
                            contender.CustomImageUrl = thumb;

                            _election = await electionService.GetElection(_election.Id);
                            _controller.Election = _election;
                            try
                            {
                                contender.SubmissionEmbedId =
                                    (long)(await args.Channel.SendMessageAsync(null, false, contender.GetEmbed())).Id;
                                _controller.UpdateOpeningMessage();
                            }
                            catch (Exception e)
                            {
                                //shouldn't happen but
                                _exceptionHandler.Handle(e, args);
                            }
                        }
                    }
                }

            }
            finally
            {
                await args.Message.DeleteAsync();
                _addContenderSemaphore.Release();
            }
        }

        [CommandHandler(Regex = @"vote \d+ [1,2,3]",
            HelpText = "Submit your vote in given bracket. Can be used once per bracket. Cannot be undone.",
            HumanReadableCommand = "vote <bracketNumber> <contestantNumber>")]
        public async Task Vote(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            await _voteSemaphore.WaitAsync();
            try
            {
                using (var userService = _userService.ObtainLifetimeHandle(executionContext))
                using (var electionService = _electionService.ObtainLifetimeHandle(executionContext))
                {
                    //prepare parameters
                    var parameters = args.Message.Content.Split(' ');
                    var bracketId = int.Parse(parameters[1]);
                    var contenderId = int.Parse(parameters[2]);

                    //obtain entities
                    _election = await electionService.GetElection(_election.Id);

                    if (_election.CurrentState == Election.State.Voting)
                    {

                        var user = await userService.GetOrCreateUser(args.Author);
                        var bracket = _election.BracketStages.Last().Brackets
                            .FirstOrDefault(b => b.Number == bracketId);

                        if (bracket == null)
                        {
                            await args.Channel.SendTimedMessage("Invalid bracket number.");
                        }
                        else if (bracket.Votes.Any(vote => vote.User.Id == user.Id)
                        ) //if user has already voted let's return
                        {
                            await args.Channel.SendTimedMessage("You have already voted in this bracket.");
                        }
                        else
                        {
                            WaifuContender contender;
                            if (contenderId == 1)
                                contender = bracket.FirstContender;
                            else if (contenderId == 2)
                                contender = bracket.SecondContender;
                            else
                                contender = bracket.ThirdContender;

                            bracket.Votes.Add(new Vote
                            {
                                Bracket = bracket,
                                Contender = contender,
                                CreateDate = DateTime.UtcNow,
                                User = user
                            });

                            await _channel.SendTimedMessage(
                                $"Thanks for submitting your vote for {contender.Waifu.Name}");

                            _controller.Election = _election;
                            _controller.UpdateOpeningMessage();
                        }
                    }
                }
                await args.Message.DeleteAsync();
            }
            finally
            {
                _voteSemaphore.Release();
            }
        }

        [CommandHandler(Regex = @"remove contender \d+", HumanReadableCommand = "remove contender <waifuId>", HelpText = "Removes your contender from election. Only usable during submission stage.")]
        public async Task RemoveContender(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            try
            {
                using (var contenderService = _contenderService.ObtainLifetimeHandle(executionContext))
                using (var electionService = _electionService.ObtainLifetimeHandle(executionContext))
                {
                    var param = args.Message.Content.Split(' ');
                    _election = await electionService.GetElection(_election.Id);
                    if (_election.CurrentState == Election.State.Submission)
                    {
                        var contender =
                            _election.Contenders.FirstOrDefault(
                                waifuContender => waifuContender.Waifu.MalId == param[2]);

                        if (contender != null)
                        {
                            if (contender.Proposer.DiscordId != args.Author.Id && !executionContext.AuthenticatedCaller)
                            {
                                await args.Channel.SendTimedMessage(
                                    "You can't remove contenders proposed by other people.");
                                return;
                            }

                            if (contender.SubmissionEmbedId != 0)
                            {
                                try
                                {
                                    await (await args.Channel.GetMessageAsync((ulong)contender.SubmissionEmbedId))
                                        .DeleteAsync();
                                }
                                catch (Exception e)
                                {

                                }
                            }
                            contenderService.Remove(contender);
                        }
                        else
                        {
                            await args.Channel.SendTimedMessage(
                                "No contender of specified id has been found.");
                        }
                    }
                    else
                    {
                        await args.Channel.SendTimedMessage(
                            "You can remove contenders only during *submission* stage.");
                    }
                }
            }
            finally
            {
                await args.Message.DeleteAsync();
            }

        }
        
        #region Debug

        [CommandHandler(Regex = @"start",Debug = true)]
        public async Task Start(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var electionService = _electionService.ObtainLifetimeHandle(executionContext))
            {
                _controller.Election = await electionService.GetElection(_election.Id);
                await _controller.TransitionToVoting();
            }
        }

        [CommandHandler(Regex = @"close",Debug = true)]
        public async Task Close(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var electionService = _electionService.ObtainLifetimeHandle(executionContext))
            using (var userService = _userService.ObtainLifetimeHandle(executionContext))
            {
                _controller.Election = await electionService.GetElection(_election.Id);
                await _controller.CloseCurrentStage(userService);
            }

        }

        [CommandHandler(Regex = @"random",Debug = true)]
        public async Task AddRandomContenders(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var userService = _userService.ObtainLifetimeHandle(executionContext))
            using (var contenderService = _contenderService.ObtainLifetimeHandle(executionContext))
            using (var electionService = _electionService.ObtainLifetimeHandle(executionContext))
            using (var waifuService = _waifuService.ObtainLifetimeHandle(executionContext))
            {
                var user = await userService.GetOrCreateUser(args.Author);
                _election = await electionService.GetElection(_election.Id);

                foreach (var id in new[] {"48391", "13701", "20626", "64167", "118763" , "99441" })
                {

                    var waifu = await waifuService.GetOrCreateWaifu(id);
                    var contender = contenderService.CreateContender(user, waifu, _election);

                    try
                    {
                        await args.Message.DeleteAsync();
                    }
                    catch (Exception e)
                    {

                    }

                    await args.Channel.SendMessageAsync(null, false, contender.GetEmbed());
                    await Task.Delay(100);
                }
            }
        }

        #endregion

        [CommandHandler(FallbackCommand = true)]
        public async Task FallbackCommand(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            if (!args.Author.IsMe())
                await args.Message.DeleteAsync();
        }

        #region Moderation

        [CommandHandler(Regex = @"recreate contender \d+", Debug = true)]
        public async Task RecreateContenderEmbed(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            try
            {
                using (var electionService = _electionService.ObtainLifetimeHandle(executionContext))
                {
                    
                    _election = await electionService.GetElection(_election.Id);
                    var param = args.Message.Content.Split(' ');
                    var first = _election.Contenders.FirstOrDefault(c => c.Waifu.MalId == param[2]);

                    if (first != null)
                    {
                        if (first.SubmissionEmbedId != 0)
                        {
                            try
                            {
                                await (await args.Channel.GetMessageAsync((ulong)first.SubmissionEmbedId)).DeleteAsync();
                            }
                            catch (Exception e)
                            {

                            }
                            
                        }
                        first.SubmissionEmbedId =
                            (long)(await args.Channel.SendMessageAsync(null, false, first.GetEmbed())).Id;
                    }

                }
            }
            catch (Exception e)
            {

            }
            finally
            {
                await args.Message.DeleteAsync();
            }
        }

        [CommandHandler(Regex = @"init", Debug = true)]
        public async Task ReinitializeElection(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            try
            {
                using (var electionService = _electionService.ObtainLifetimeHandle(executionContext))
                {
                    _election = await electionService.GetElection(_election.Id);
                    _controller.Election = _election;
                    _controller.Initialize();
                }
            }
            catch (Exception e)
            {

            }
            finally
            {
                await args.Message.DeleteAsync();
            }

        }



        #endregion

        #endregion

        private async void OnHourPassed()
        {
            try
            {
                using (var electionService = _electionService.ObtainLifetimeHandle(ResourceLocator.ExecutionContext))
                using (var userService = _userService.ObtainLifetimeHandle(ResourceLocator.ExecutionContext))
                {
                    _election = await electionService.GetElection(_election.Id);
                    _controller.Election = _election;
                    if(_election.CurrentState != Election.State.Closed && _election.CurrentState != Election.State.ClosedForcibly)
                        await _controller.ProcessTimePass(userService);
                }
            }
            catch (Exception e)
            {
                _exceptionHandler.Handle(e);
            }

        }

        public void OnCreated()
        {
            _controller.Initialize();
        }

        public void DeregisterTimer()
        {
            TimerService.Instance.Deregister(_timerRegistration);
        }
    }
}
