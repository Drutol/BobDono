using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    [Module(Name = "ElectionThemes(Channel)",Description = "Allows to submit theme ideas for elections",IsChannelContextual = true)]
    public class ElectionThemesContext : ContextModuleBase
    {
        private const string ApprovalsKey = "Approvals";
        private const string ApprovalStatusKey = "Approval Status";

        public sealed override ulong? ChannelIdContext { get; protected set; }

        private readonly CustomDiscordClient _discordClient;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly IUserService _userService;
        private readonly IElectionThemeService _electionThemeService;

        private DiscordChannel _channel;

        public ElectionThemesContext(ElectionThemeChannel channel, DiscordClient discordClient,
            IExceptionHandler exceptionHandler, IUserService userService, IElectionThemeService electionThemeService)
        {
            _discordClient = discordClient as CustomDiscordClient;
            _exceptionHandler = exceptionHandler;
            _userService = userService;
            _electionThemeService = electionThemeService;
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

        private void OnTimePass()
        {

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
                        themeService.ConfigureIncludes().WithChain(query => query.Include(t => t.Approvals)).Commit();
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

                    themeService.ConfigureIncludes().WithChain(q => q.Include(theme => theme.Proposer));
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
                        CreateDate = DateTime.UtcNow
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

        private async Task UpdateThemeMessage(ElectionTheme theme)
        {
            var message = await _channel.GetMessageAsync((ulong)theme.DiscordMessageId);

            var embed = new DiscordEmbedBuilder(message.Embeds.First());

            var approved = theme.Approvals.Count >= 1;

            embed.Color = approved ? DiscordColor.SpringGreen : DiscordColor.Gray;

            embed.Fields.First(field => field.Name.Equals(ApprovalsKey)).Value = string.Join(",", theme.Approvals
                .Select(a => a.User.Name));
            embed.Fields.First(field => field.Name.Equals(ApprovalStatusKey)).Value =
                approved ? ":white_check_mark:" : ":hourglass:";

            await message.ModifyAsync(default, new Optional<DiscordEmbed>(embed.Build()));
        }

        public async void OnCreate()
        {
            var embed = new DiscordEmbedBuilder();

            embed.Title = "What's going on?";
            embed.Description =
                "This is place for proposing future election themes, I'll take random one on every 1st and 15th day of the month and start election\n\n" +
                $"You can add theme using command:\n`{CommandHandlerAttribute.CommandStarter}add theme <name>\\<description>\n\n`" +
                "Each theme needs to be approved by at least 3 people using reactions, once you approve something it cannot be undone. " +
                "Every person can add up to 3 themes. " +
                "If theme doersn't get 3 approvals it will be deleted after a week.";
                
            embed.Color = DiscordColor.Gray;
            await _channel.SendMessageAsync(null, false, embed.Build());
        }

        private async void ClearChannel()
        {
            var messages = await _channel.GetMessagesAsync();

            foreach (var message in messages)
            {
                if (!message.Author.IsMe())
                    await message.DeleteAsync();
            }
        }

        [CommandHandler(FallbackCommand = true)]
        public async Task FallbackCommand(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            if (!args.Author.IsMe())
                await args.Message.DeleteAsync();
        }
    }
}
