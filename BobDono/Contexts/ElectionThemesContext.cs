using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Autofac;
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
using BobDono.Modules;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Contexts
{
    [Module(Name = "ElectionThemes(Channel)",Description = "Allows to submit theme ideas for elections",IsChannelContextual = true)]
    public class ElectionThemesContext : ContextModuleBase
    {
        private const string ApprovalsKey = "Approvals";
        private const string ApprovalStatusKey = "Approval Status";
        private const string NextElectionKey = "Next election";
        private const int ApprovalsCount = 5;

        public override DiscordChannel Channel => _channel;

        private readonly CustomDiscordClient _discordClient;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly IServiceFactory<IUserService> _userService;
        private readonly IServiceFactory<IElectionThemeService> _electionThemeService;
        private readonly IServiceFactory<IElectionThemesChannelService> _electionThemesChannelService;
        private readonly IServiceFactory<IElectionService> _electionService;


        private Random _random = new Random();
        private DiscordChannel _channel;
        private ElectionThemeChannel _themesChannel;

        public ElectionThemesContext(ElectionThemeChannel channel, DiscordClient discordClient,
            IExceptionHandler exceptionHandler, IServiceFactory<IUserService> userService,
            IServiceFactory<IElectionThemeService> electionThemeService,
            IServiceFactory<IElectionThemesChannelService> electionThemesChannelService,
            IServiceFactory<IElectionService> electionService) : base((ulong) channel.DiscordChannelId)
        {
            _discordClient = discordClient as CustomDiscordClient;
            _themesChannel = channel;
            _exceptionHandler = exceptionHandler;
            _userService = userService;
            _electionThemeService = electionThemeService;
            _electionThemesChannelService = electionThemesChannelService;
            _electionService = electionService;
            ChannelIdContext = (ulong) channel.DiscordChannelId;

            var guild = ResourceLocator.DiscordClient.GetNullsGuild();
            _channel = _discordClient.GetChannel(guild, (ulong) channel.DiscordChannelId);

            if (_channel == null)
                throw new InvalidOperationException("Discord channel is invalid");

            _discordClient.MessageReactionAdded += DiscordClientOnMessageReactionAdded;
            TimerService.Instance.Register(new TimerService.TimerRegistration
            {
                Interval = TimeSpan.FromDays(1),
                Task = OnTimePass
            }.FireOnNextFullDay());

            ClearChannel();
        }

        public async void OnTimePass()
        {
            try
            {
                var ctx = ResourceLocator.ExecutionContext;
                using (var themeService = _electionThemeService.ObtainLifetimeHandle(ctx))
                using (var themeChannelService = _electionThemesChannelService.ObtainLifetimeHandle(ctx))
                using (var electionService = _electionService.ObtainLifetimeHandle(ctx))
                using (var userService = _userService.ObtainLifetimeHandle(ctx))
                {
                    var themes = await themeService.GetAllAsync();
                    foreach (var electionTheme in themes)
                    {
                        if (DateTime.UtcNow - electionTheme.CreateDate > TimeSpan.FromDays(7) &&
                            !electionTheme.Approved)
                        {
                            await (await _channel.GetMessageAsync((ulong) electionTheme.DiscordMessageId))
                                .DeleteAsync();
                            themeService.Remove(electionTheme);
                        }
                    }

                    _themesChannel = await themeChannelService.FirstAsync(channel =>
                        channel.DiscordChannelId == (long) ChannelIdContext.Value);

                    if (DateTime.UtcNow > _themesChannel.NextElection)
                    {
                        await StartRandomElection(electionService,themeService,userService);
                    }

                    await UpdateOpeningMessage();
                }
            }
            catch (Exception e)
            {
                _exceptionHandler.Handle(e);
            }
        }

        private async Task StartRandomElection(IElectionService electionService, IElectionThemeService themeService, IUserService userService)
        {
            _themesChannel.NextElection = DateTime.Today.GetNextElectionDate();

            themeService.ConfigureIncludes()
                .WithChain(q => q.Include(theme => theme.Proposer).ThenInclude(user => user.Elections)).Commit();
            var themes = await themeService.GetAllWhereAsync(theme => theme.Approved && !theme.Used);

            if (themes.Any())
            {

                var randomTheme = themes[_random.Next(themes.Count)];
                var user = await userService.FirstAsync(u => u.Id == randomTheme.Proposer.Id);
                randomTheme.Used = true;
                randomTheme.ElectionCreateDate = DateTime.UtcNow;

                var election = new Election
                {
                    Name = randomTheme.Title,
                    Description = randomTheme.Description,
                    SubmissionsStartDate = DateTime.UtcNow,
                    SubmissionsEndDate = DateTime.Today.AddHours(DateTime.UtcNow.Hour + 1).AddDays(2),
                    EntrantsPerUser = 2,
                    FeatureImageRequired = true,
                };
                election.VotingStartDate = election.SubmissionsEndDate.AddHours(2);

                var guild = _discordClient.GetNullsGuild();
                var category = await guild.GetCategoryChannel(DiscordClientExtensions.ChannelCategory.Elections);
                var electionChannel = await guild.CreateChannelAsync(election.Name, ChannelType.Text,
                    category,
                    null, null, null,
                    election.Description);
                election.DiscordChannelId = electionChannel.Id;
                (ResourceLocator.DiscordClient as CustomDiscordClient).CreatedChannels.Add(electionChannel);
                await electionService.CreateElection(election, user);
                await electionService.SaveChangesAsync();

                election = await electionService.FirstAsync(e => e.DiscordChannelId == electionChannel.Id);
                using (var dependencyScope = ResourceLocator.ObtainScope())
                {
                    var electionContext = dependencyScope.Resolve<ElectionContext>(new TypedParameter(typeof(Election),election));
                    electionContext.OnCreated();
                    dependencyScope.Resolve<ElectionsModule>().ElectionsContexts.Add(electionContext);
                }
            }
        }

        private async Task DiscordClientOnMessageReactionAdded(MessageReactionAddEventArgs args)
        {
            try
            {

                if (args.Emoji.Name == "👍" &&
                    args.Channel.Id == ChannelIdContext && 
                    !args.User.IsMe() && !args.User.IsBot)
                {
                    var ctx = ResourceLocator.ExecutionContext;
                    using (var userService = _userService.ObtainLifetimeHandle(ctx))
                    using (var themeService = _electionThemeService.ObtainLifetimeHandle(ctx))
                    {
                        themeService.ConfigureIncludes().WithChain(query => query.Include(t => t.Approvals).ThenInclude(ut => ut.User)).Commit();
                        var theme = await themeService.FirstAsync(electionTheme =>
                            electionTheme.DiscordMessageId == (long) args.Message.Id);

                        //some other message
                        if(theme == null)
                            return;

                        var user = await userService.GetOrCreateUser(args.User);

                        //if user already approved theme
                        if (theme.Approvals.Any(userTheme => userTheme.User.Equals(user)))
                            return;

                        //else we can add approval
                        theme.Approvals.Add(new UserTheme {Theme = theme, User = user});

                        if (theme.Approvals.Count >= ApprovalsCount)
                            theme.Approved = true;

                        await UpdateThemeMessage(theme);
                    }
                }
            }
            catch (Exception e)
            {
                _exceptionHandler.Handle(e);
            }
        }

        [CommandHandler(Regex = "add theme .*\n.*",HumanReadableCommand = "add theme <name>\\n<description>",HelpText = "Adds theme, name will be used as channel name for future election therefore it must be alphanumeric. Description is up to 500 characters.")]
        public async Task CreateNewElectionTheme(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            try
            {
                using (var userService = _userService.ObtainLifetimeHandle(executionContext))
                using (var themeService = _electionThemeService.ObtainLifetimeHandle(executionContext))
                {
                    var user = await userService.GetOrCreateUser(args.Author);

                    themeService.ConfigureIncludes().WithChain(q => q.Include(theme => theme.Proposer)).Commit();
                    var currentCount = (await themeService.GetAllWhereAsync(theme => theme.Proposer.Equals(user) && !theme.Used)).Count;

                    if (currentCount >= 3)
                    {
                        await args.Channel.SendTimedMessage(
                            "You have already submitted three themes which were not yet used.");
                        return;
                    }

                    var command = args.Message.Content.Replace($"{CommandHandlerAttribute.CommandStarter}add theme ", "");
                    var pos = command.IndexOf('\n');

                    var electionTheme = new ElectionTheme()
                    {
                        Title = command.Substring(0, pos).Replace(" ", "-"),
                        Description = command.Substring(pos + 1),
                        CreateDate = DateTime.UtcNow,
                        Proposer = user
                    };

                    if (electionTheme.Title.Length < 2 || !Regex.IsMatch(electionTheme.Title, "^[a-zA-Z0-9_-]*$"))
                    {
                        await args.Channel.SendTimedMessage(
                            "Provided name is invalid.");
                        return;
                    }

                    if (electionTheme.Description.Length > 500)
                    {
                        await args.Channel.SendTimedMessage(
                            $"Too long description `{electionTheme.Description.Length}/500`");
                        return;
                    }

                    var embed = new DiscordEmbedBuilder()
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            IconUrl = args.Message.Author.AvatarUrl,
                            Name = args.Message.Author.Username,
                        },
                        Color = DiscordColor.Gray,
                        Description = electionTheme.Description,
                        Title = electionTheme.Title
                    };

                    embed.AddField(ApprovalsKey, "-", true);
                    embed.AddField(ApprovalStatusKey, ":hourglass:", true);

                    var msg = await args.Channel.SendMessageAsync(null, false, embed);
                    electionTheme.DiscordMessageId = (long)msg.Id;

                    themeService.Add(electionTheme);

                    await msg.CreateReactionAsync(DiscordEmoji.FromName(_discordClient, ":thumbsup:"));
                }
            }
            finally
            {
                await args.Message.DeleteAsync();
            }

        }

        #region Debug

        [CommandHandler(Regex = "pass",Debug = true)]
        public async Task DayPass(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            OnTimePass();
        }

        [CommandHandler(Regex = "updateopeningmessage",Debug = true)]
        public async Task UpdateOpeningMessage(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            try
            {
                var message = await _channel.GetMessageAsync((ulong) _themesChannel.OpeningMessageId);
                var embed = new DiscordEmbedBuilder(message.Embeds.First());

                embed.Title = "What's going on?";
                embed.Description =
                    "This is place for proposing future election themes, I'll take random one on every 1st and 15th day of the month and start election\n\n" +
                    $"You can add theme using command:\n`{CommandHandlerAttribute.CommandStarter}add theme <name>\\n<description>`\n\n" +
                    $"Each theme needs to be approved by at least {ApprovalsCount} people using reactions, once you approve something it cannot be undone. " +
                    "Every person can add up to 3 themes. " +
                    $"If theme doersn't get {ApprovalsCount} approvals it will be deleted after a week.";

                await message.ModifyAsync(default, new Optional<DiscordEmbed>(embed.Build()));
            }
            catch (Exception)
            {
                //somebody removed a message
                //Fox?
            }
            finally
            {
                await args.Message.DeleteAsync();
            }
        }

        #endregion

        private async Task UpdateThemeMessage(ElectionTheme theme)
        {
            try
            {
                var message = await _channel.GetMessageAsync((ulong) theme.DiscordMessageId);
                var embed = new DiscordEmbedBuilder(message.Embeds.First());

                embed.Color = theme.Approved ? DiscordColor.SapGreen : DiscordColor.Gray;

                embed.Fields.First(field => field.Name.Equals(ApprovalsKey)).Value = string.Join(", ", theme.Approvals
                    .Select(a => a.User.Name));
                embed.Fields.First(field => field.Name.Equals(ApprovalStatusKey)).Value =
                    theme.Approved ? ":white_check_mark:" : ":hourglass:";

                await message.ModifyAsync(default, new Optional<DiscordEmbed>(embed.Build()));
            }
            catch (Exception)
            {
                //somebody removed a message
                //TODO
            }
        }

        private async Task UpdateOpeningMessage()
        {
            try
            {
                var message = await _channel.GetMessageAsync((ulong)_themesChannel.OpeningMessageId);
                var embed = new DiscordEmbedBuilder(message.Embeds.First());

                embed.Fields.First(field => field.Name.Equals(NextElectionKey)).Value =
                    $"{_themesChannel.NextElection} (UTC) *({(_themesChannel.NextElection - DateTime.UtcNow).Days} days)*";

                await message.ModifyAsync(default, new Optional<DiscordEmbed>(embed.Build()));
            }
            catch (Exception)
            {
                //somebody removed a message
                //Fox?
            }
        }

        public async Task<long> OnCreate(ElectionThemeChannel channel)
        {
            var embed = new DiscordEmbedBuilder();

            embed.Title = "What's going on?";
            embed.Description =
                "This is place for proposing future election themes, I'll take random one on every 1st and 15th day of the month and start election\n\n" +
                $"You can add theme using command:\n`{CommandHandlerAttribute.CommandStarter}add theme <name>\\n<description>`\n\n" +
                $"Each theme needs to be approved by at least {ApprovalsCount} people using reactions, once you approve something it cannot be undone. " +
                "Every person can add up to 3 themes. " +
                $"If theme doersn't get {ApprovalsCount} approvals it will be deleted after a week.";
                
            embed.Color = DiscordColor.Gray;
            embed.AddField(NextElectionKey, $"{channel.NextElection} (UTC) *({(channel.NextElection - DateTime.UtcNow).Days} days)*");

            return (long) (await _channel.SendMessageAsync(null, false, embed.Build())).Id;
        }

        [CommandHandler(FallbackCommand = true)]
        public async Task FallbackCommand(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            if (!args.Author.IsMe())
                await args.Message.DeleteAsync();
        }
    }
}
