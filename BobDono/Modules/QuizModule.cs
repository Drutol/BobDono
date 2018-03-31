using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BobDono.Core.Attributes;
using BobDono.Core.Extensions;
using BobDono.Core.Utils;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;
using BobDono.Models.Entities;
using BobDono.Utils;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Modules
{
    [Module(Name = "Quiz Module", Description = "Various Quiz related commands.")]
    public class QuizModule
    {
        private readonly Random _random = new Random();
        private readonly HashSet<ulong> _runningQuzies = new HashSet<ulong>();

        private readonly IServiceFactory<IQuizService> _quizService;
        private readonly IServiceFactory<IUserService> _userService;
        private readonly IServiceFactory<IQuizQuestionService> _quizQuestionService;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly CustomDiscordClient _customDiscordClient;

        public QuizModule(IServiceFactory<IQuizService> quizService, IServiceFactory<IUserService> userService,
            IServiceFactory<IQuizQuestionService> quizQuestionService, IExceptionHandler exceptionHandler,
            CustomDiscordClient customDiscordClient)
        {
            _quizService = quizService;
            _userService = userService;
            _quizQuestionService = quizQuestionService;
            _exceptionHandler = exceptionHandler;
            _customDiscordClient = customDiscordClient;
        }

        [CommandHandler(Regex = "quiz start", HumanReadableCommand = "quiz start", HelpText = "Starts new quiz game.",
            Awaitable = false)]
        public async Task StartSession(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            if(_runningQuzies.Contains(args.Author.Id))
                return;

            _runningQuzies.Add(args.Author.Id);
            using (var userService = _userService.ObtainLifetimeHandle(executionContext))
            using (var quizQuestionService = _quizQuestionService.ObtainLifetimeHandle(executionContext))
            {
                //for (int i = 0; i < 10; i++)
                //{
                //    quizQuestionService.Add(new QuizQuestion
                //    {
                //        Answers = new[] {i.ToString()},
                //        Author = "d",
                //        CreatedDate = DateTime.UtcNow,
                //        Hint = "nope",
                //        Points = _random.Next(0, 4),
                //        Question = i.ToString(),
                //        Set = QuizSession.QuizQuestionSet.Trivia,
                //        QuestionBatch = 1,
                //    });
                //}
                //return;

                userService.ConfigureIncludes().WithChain(query =>
                    query.Include(u => u.QuizSessions).ThenInclude(sessions => sessions.Answers)).Commit();
                var user = await userService.GetOrCreateUser(args.Author);


                var cts = new CancellationTokenSource();
                var timeout = TimeSpan.FromMinutes(1);
                var guild = _customDiscordClient.GetNullsGuild();
                var member = await guild.GetMemberAsync(args.Author.Id);
                var channel = await member.CreateDmChannelAsync();

                var session = await PrepareGameSession(user,quizQuestionService);

                if (session.QuestionWorkSet.Count == 0)
                {
                    await channel.SendMessageAsync("Looks like I don't have any new questions for you :(");
                    return;
                }

                await channel.SendMessageAsync(null, false, GetWelcomeEmbed(user,session));
                try
                {
                    if (await channel.GetNextValidResponse("Type 'start' when you are ready!",
                        s => Task.FromResult(s == "start"),
                        timeout,
                        cts.Token))
                    {
                        while (session.QuestionWorkSet.Any())
                        {
                            var questionSet = PickQuestionsFromSet(session.QuestionWorkSet);
                            var categoryEmbed = GetSelectCategoryEmbed(session, questionSet);
                            await channel.SendMessageAsync(null, false, categoryEmbed);

                            var selection = await channel.GetNextValidResponse("Which question do you choose?",
                                s =>
                                {
                                    if (int.TryParse(s, out var index) && index < questionSet.Count)
                                        return Task.FromResult(index);
                                    throw new ArgumentOutOfRangeException();
                                }, timeout, cts.Token);

                            var questionEmbed = GetQuestionEmbed(questionSet[selection]);
                            await channel.SendMessageAsync(null, false, questionEmbed);

                            var answer = await channel.GetNextValidResponse("Your answer?", Task.FromResult,
                                timeout, cts.Token);

                            var distances = questionSet[selection].Answers
                                .Select(s =>
                                    (100 * LevenshteinDistance.Compute(answer.ToLower(), s.ToLower().Trim())) /
                                    answer.Length);

                            var questionAnswer = new QuizAnswer
                            {
                                Session = session,
                                Question = questionSet[selection],
                                Answer = answer,
                                IsCorrect = distances.Any(i => i <= 10)
                            };

                            await Task.Delay(500);

                            var resultEmbed = GetResultEmbed(questionAnswer);
                            await channel.SendMessageAsync(null, false, resultEmbed);

                            if (questionAnswer.IsCorrect)
                            {
                                session.QuestionWorkSet.Remove(questionSet[selection]);
                                session.Score += questionSet[selection].Points;
                                if (session.QuestionWorkSet.Count == 0)
                                {
                                    session.Status = QuizSession.QuizStatus.Finished;
                                    session.CompletedBatch = session.SessionBatch;
                                    await channel.SendMessageAsync(null, false, GetSuccessFinishEmbed(session));
                                    break;
                                }
                            }
                            else
                            {
                                session.RemainingChances--;
                                if (session.RemainingChances == 0)
                                {
                                    session.Status = QuizSession.QuizStatus.Finished;
                                    await channel.SendMessageAsync(null, false, GetFailedFinishEmbed(session));
                                    break;
                                }
                            }

                            session.Answers.Add(questionAnswer);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    session.Status = QuizSession.QuizStatus.TimedOut;
                }
                catch (Exception e)
                {
                    session.Status = QuizSession.QuizStatus.Errored;
                    await channel.SendMessageAsync(_exceptionHandler.Handle(e));
                }

                session.Finished = DateTime.UtcNow;
                user.QuizSessions.Add(session);
            }
            _runningQuzies.Remove(args.Author.Id);
        }


        private DiscordEmbed GetWelcomeEmbed(User user, QuizSession session)
        {
            var builder = new DiscordEmbedBuilder
            {
                Title = "Quiz time!",
                Color = DiscordColor.Gold,
                Description =
                    "Welcome, we are going to play a quiz game with some trivia questions about The Nulls of MAL server ^^." +
                    " Getting high scores will grant you badges and OneDrive link for @Drutol's anime music :D" +
                    " Rules are simple:\n\n" +
                    "**1.** You are presented with one question at the time, you have 1 minute to answer.\n" +
                    "**2.** Questions require 90% of answer accuracy, levenshtein distance is used for this calculation.\n" +
                    "**3.** Question may have several answers.\n" +
                    "**4.** After answering correctly you will be presented with 3 points categories to choose from. The more points the harder the question.\n" +
                    "**5.** Questions are divided in batches, depending on when they were added.\n" +
                    " Completing one batch will finish the game and starting new session will grant you all points from previous batches\n" +
                    "**6.** You can answer incorrectly 3 times. Session ends on 4th incorrent answer.\n" +
                    "**7.** You will have to answer correctly for all questions in one session to pass the batch.\n" +
                    "**8.** Questions are chosen randomly.\n"
            };

            var bestSession = user.QuizSessions.OrderByDescending(s => s.Score).FirstOrDefault();
            if (bestSession != null)
            {
                builder.AddField("Best session:", $"Highscore: {bestSession.Score}\nDate: {bestSession.Finished:D}");
            }


            builder.AddField("Starting question batch:", $"{session.SessionBatch}");


            return builder;
        }

        private DiscordEmbed GetSelectCategoryEmbed(QuizSession session, List<QuizQuestion> set)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Orange,
                Title = $"Select category. ({session.Answers.Count}/{session.QuestionsCount}",
                Description = "",
            };

            for (int i = 0; i < set.Count; i++)
                embed.Description += $"{i}. Question for {set[i].Points} points.\n";
            
            var lives = "";
            for (int i = 0; i < session.RemainingChances; i++)
                lives += ":green_heart:";
            //for (int i = 0; i < session.TotalChances - session.RemainingChances; i++)
            //    lives += ":broken_heart:";

            embed.AddField("Remaining chances:", lives);

            return embed;
        }

        private DiscordEmbed GetQuestionEmbed(QuizQuestion question)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Orange,
                Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"Author: {question.Author}"},
                Title = $"Question for {question.Points} points. (Id: {question.Id})",
                Description = question.Question,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"Added on: {question.CreatedDate:d}"}
            };

            if (!string.IsNullOrEmpty(question.Hint))
                embed.AddField("Hint:", question.Hint);

            return embed;
        }

        private DiscordEmbed GetResultEmbed(QuizAnswer answer)
        {
            if (answer.IsCorrect)
            {
                return new DiscordEmbedBuilder
                {
                    Color = DiscordColor.SpringGreen,
                    Title = "Correct!",
                    Description = "Good job! Let's move onto the next one :)"
                };
            }

            return new DiscordEmbedBuilder
            {
                Color = DiscordColor.DarkRed,
                Title = "Whoopsies!",
                Description = "Well, you've made a mistake :("
            };
        }

        private DiscordEmbed GetSuccessFinishEmbed(QuizSession session)
        {
            var embed =  new DiscordEmbedBuilder
            {
                Color = DiscordColor.Green,
                Title = "Wow you did it!",
                Description = "You actually did it! Congrats!"
            };
            embed.AddField("Questions answered:", session.Answers.Count.ToString());
            embed.AddField("Points scored:", session.Score.ToString());
            return embed;
        }

        private DiscordEmbed GetFailedFinishEmbed(QuizSession session)
        {
            var embed =  new DiscordEmbedBuilder
            {
                Color = DiscordColor.Red,
                Title = "That's unfortunate...",
                Description = "You have run out of chances. The game will now finish."
            };
            embed.AddField("Questions answered:", session.Answers.Count.ToString());
            embed.AddField("Points scored:", session.Score.ToString());
            return embed;
        }

        private List<QuizQuestion> PickQuestionsFromSet(List<QuizQuestion> set)
        {
            var groups = set.GroupBy(question => question.Points).OrderBy(grouping => grouping.Key).ToList();
            var output = new List<QuizQuestion>();
            for (int i = 0; i < (set.Count > 3 ? 3 : set.Count); i++)
            {
                IGrouping<int, QuizQuestion> pickedGroup;
                //ensure one easy question
                if (!output.Any())
                    pickedGroup = groups.First();               
                else
                    pickedGroup = groups[_random.Next(0, groups.Count)];

                output.Add(pickedGroup.ElementAt(_random.Next(0, pickedGroup.Count())));
            }

            return output;
        }

        private async Task<QuizSession> PrepareGameSession(User user, IQuizQuestionService questionService)
        {
            var session = new QuizSession
            {
                User = user,
                Started = DateTime.UtcNow,
                RemainingChances = 3,
                TotalChances = 3,
                Status = QuizSession.QuizStatus.InProgress,
                Set = QuizSession.QuizQuestionSet.Trivia,                                     
            };

            var highestSession = user.QuizSessions.Where(s => s.CompletedBatch != null)
                .OrderByDescending(s => s.CompletedBatch.Value).FirstOrDefault();

            session.SessionBatch = highestSession?.CompletedBatch + 1 ?? 1;
            session.QuestionWorkSet = await
                questionService.GetAllWhereAsync(question => question.QuestionBatch == session.SessionBatch);
            session.QuestionWorkSet = session.QuestionWorkSet.Take(5).ToList();
            session.QuestionsCount = session.QuestionWorkSet.Count;

            if (session.SessionBatch != 1)
            {
                session.AdditonalScoreFromPreviousBatches =
                    (await questionService.GetAllWhereAsync(question => question.QuestionBatch < session.SessionBatch))
                    .Sum(question => question.Points);
            }

            return session;
        }
    }
}
