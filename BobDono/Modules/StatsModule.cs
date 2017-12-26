using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BobDono.Core.Attributes;
using BobDono.Core.BL;
using BobDono.Core.Utils;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;
using BobDono.Models.Entities.Stats;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace BobDono.Modules
{
    [Module(Name = "Stats",Description = "BobStats:tm:")]
    public class StatsModule
    {
        private readonly IServiceFactory<IExceptionReportsService> _exceptionService;
        private readonly IServiceFactory<IExecutedCommandsService> _executedCommandsService;
        private readonly IServiceFactory<IVoteService> _voteServiceFactory;
        private readonly IServiceFactory<IUserService> _userService;
        private readonly IServiceFactory<IContenderService> _contendersService;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly CustomDiscordClient _client;

        private readonly DateTime _startedAt = DateTime.UtcNow;
        private DateTime? _lastStatsGeneration;
        private DiscordEmbed _cachedEmbed;

        private List<ExecutedCommand> _commandBacklog;
        private MD5 _md5Hasher;

        public StatsModule(IServiceFactory<IExceptionReportsService> exceptionService,
            IServiceFactory<IExecutedCommandsService> executedCommandsService,
            IServiceFactory<IVoteService> voteServiceFactory, IServiceFactory<IUserService> userService,
            IServiceFactory<IContenderService> contendersService, IExceptionHandler exceptionHandler)
        {
            _exceptionService = exceptionService;
            _executedCommandsService = executedCommandsService;
            _voteServiceFactory = voteServiceFactory;
            _userService = userService;
            _contendersService = contendersService;
            _exceptionHandler = exceptionHandler;

            TimerService.Instance.Register(new TimerService.TimerRegistration
            {
                DueTime = TimeSpan.FromMinutes(10),
                Interval = TimeSpan.FromMinutes(10),
                Task = OnTimePass
            });

            Messenger.Instance.Register<ExecutedCommand>(c => OnNewCommands(c));
            
            _commandBacklog = new List<ExecutedCommand>();
            _md5Hasher = MD5.Create();
        }

        private void OnNewCommands(ExecutedCommand executedCommand)
        {
            if (executedCommand.CommandName != null && executedCommand.CallerName != null)
            {             
                executedCommand.CommandHash = BitConverter.ToInt32(_md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(executedCommand.CommandName)), 0);
                executedCommand.CallerHash = BitConverter.ToInt32(_md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(executedCommand.CallerName)), 0);
                _commandBacklog.Add(executedCommand);
            }
        }


        private void OnTimePass()
        {
            try
            {
                using (var es = _executedCommandsService.ObtainLifetimeHandle())
                {
                    es.AddRange(_commandBacklog);
                }
            }
            catch (Exception e)
            {
                _exceptionHandler.Handle(e, "Commands stats generation");
            }
            finally
            {
                _commandBacklog.Clear();
            }
        }

        [CommandHandler(Regex = "stats",HumanReadableCommand = "stats",HelpText = "Gets various stats about the bot.")]
        public async Task GetStats(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            if (_lastStatsGeneration != null)
            {
                if (DateTime.UtcNow - _lastStatsGeneration < TimeSpan.FromMinutes(10))
                {
                    await args.Channel.SendMessageAsync(null, false, _cachedEmbed);
                    return;
                }
            }

            var topCallers = new StringBuilder();
            var topImaginedCommands = new StringBuilder();
            var topCommands = new StringBuilder();
            var otherStats = new StringBuilder();

            using (var es = _executedCommandsService.ObtainLifetimeHandle(executionContext))
            using (var us = _userService.ObtainLifetimeHandle(executionContext))
            using (var cs = _contendersService.ObtainLifetimeHandle(executionContext))
            using (var vs = _voteServiceFactory.ObtainLifetimeHandle(executionContext))
            using (var ex = _exceptionService.ObtainLifetimeHandle(executionContext))
            {
                var groups = es.GetGrouping(command => command.CommandHash,true);
                int i = 1;
                foreach (var cmds in groups.OrderByDescending(commands => commands.Count()).Take(5))
                {
                    topCommands.AppendLine($"`{i++}`. **{cmds.First().CommandName.Split('<').First()}** - *{cmds.Count()}*");
                }

                i = 1;
                groups = es.GetGrouping(command => command.CallerHash,true);
                foreach (var cmds in groups.OrderByDescending(commands => commands.Count()).Take(5))
                {
                    topCallers.AppendLine($"`{i++}`. **{cmds.First().CallerName}** - *{cmds.Count()}*");
                }

                i = 1;
                groups = es.GetGrouping(command => command.CommandHash,false);
                foreach (var cmds in groups.OrderByDescending(commands => commands.Count()).Take(5))
                {
                    topImaginedCommands.AppendLine($"`{i++}`. **{cmds.First().CommandName}** - *{cmds.Count()}* - *({cmds.GroupBy(command => command.CallerHash).OrderByDescending(commands => commands.Count()).FirstOrDefault()?.FirstOrDefault()?.CallerName})*");
                }


                otherStats.AppendLine($"All commands processed: **{es.Count()}**");
                otherStats.AppendLine($"Election contenders count: **{cs.Count()}**");
                otherStats.AppendLine($"Election votes count: **{vs.Count()}**");
                otherStats.AppendLine($"Users count: **{us.Count()}**");
                otherStats.AppendLine($"Exceptions happened: **{ex.Count()}**");
                otherStats.AppendLine($"Time since last restart: **{DateTime.UtcNow - _startedAt:g}**");
                otherStats.AppendLine($"Time since first commit: **{DateTime.UtcNow - new DateTime(2017,9,16):g}**");
            }

            var embed = new DiscordEmbedBuilder();
            if (topCommands.ToString().Any())
                embed.AddField("Top commands", topCommands.ToString());
            if (topCallers.ToString().Any())
                embed.AddField("Top callers", topCallers.ToString());
            if (topImaginedCommands.ToString().Any())
                embed.AddField("Top imagined commands", topImaginedCommands.ToString());

            embed.AddField("Other", otherStats.ToString());
            embed.Color = DiscordColor.Brown;

            _cachedEmbed = embed.Build();
            _lastStatsGeneration = DateTime.UtcNow;


            await args.Channel.SendMessageAsync(null, false, _cachedEmbed);
        }

        [CommandHandler(Regex = "plzfixhashes",Debug = true)]
        public async Task FixHashes(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            try
            {
                using (var ec = _executedCommandsService.ObtainLifetimeHandle(executionContext))
                {
                    foreach (var executedCommand in ec.GetAll())
                    {
                        executedCommand.CommandHash = BitConverter.ToInt32(_md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(executedCommand.CommandName)), 0);
                        executedCommand.CallerHash = BitConverter.ToInt32(_md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(executedCommand.CallerName)), 0);
                    }
                }
            }
            catch (Exception e)
            {

            }
            finally
            {

            }
        }

    }

}
