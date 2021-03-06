﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Autofac;
using BobDono.Core.Attributes;
using BobDono.Core.BL;
using BobDono.Core.Interfaces;
using BobDono.Core.Utils;
using BobDono.DataAccess;
using BobDono.DataAccess.Services;
using BobDono.Interfaces;
using BobDono.Interfaces.Queries;
using BobDono.Interfaces.Services;
using BobDono.MalHell.Comm;
using BobDono.MalHell.Queries;
using DSharpPlus;

namespace BobDono.Core
{
    public class ResourceLocator 
    {
        private static ILifetimeScope _container;


        public static void RegisterDependencies(DiscordClient client, params Type[] types)
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<BotContext>().As<IBotContext>().SingleInstance();
            builder.RegisterType<BotBackbone>().As<IBotBackbone>().SingleInstance();
            builder.RegisterType<ExceptionHandler>().As<IExceptionHandler>().SingleInstance();
            builder.RegisterType<HttpClientProvider>().As<IHttpClientProvider>().SingleInstance();
            builder.RegisterType<CharacterDetailsQuery>().As<ICharacterDetailsQuery>().SingleInstance();
            builder.RegisterType<ProfileQuery>().As<IProfileQuery>().SingleInstance();
            builder.RegisterType<CharactersSearchQuery>().As<ICharactersSearchQuery>().SingleInstance();
            builder.RegisterType<StaffDetailsQuery>().As<IStaffDetailsQuery>().SingleInstance();

            builder.RegisterType<ElectionService>().AsFactory().WithProduct<IElectionService>().SingleInstance();
            builder.RegisterType<WaifuService>().AsFactory().WithProduct<IWaifuService>().SingleInstance();
            builder.RegisterType<UserService>().AsFactory().WithProduct<IUserService>().SingleInstance();
            builder.RegisterType<TrueWaifuService>().AsFactory().WithProduct<ITrueWaifuService>().SingleInstance();
            builder.RegisterType<ContenderService>().AsFactory().WithProduct<IContenderService>().SingleInstance();
            builder.RegisterType<HallOfFameMemberService>().AsFactory().WithProduct<IHallOfFameMemberService>().SingleInstance();
            builder.RegisterType<HallOfFameChannelService>().AsFactory().WithProduct<IHallOfFameChannelService>().SingleInstance();
            builder.RegisterType<ElectionThemeService>().AsFactory().WithProduct<IElectionThemeService>().SingleInstance();
            builder.RegisterType<MatchupService>().AsFactory().WithProduct<IMatchupService>().SingleInstance();
            builder.RegisterType<ElectionThemesChannelService>().AsFactory().WithProduct<IElectionThemesChannelService>().SingleInstance();
            builder.RegisterType<MerchService>().AsFactory().WithProduct<IMerchService>().SingleInstance();
            builder.RegisterType<ExecutedCommandsService>().AsFactory().WithProduct<IExecutedCommandsService>().SingleInstance();
            builder.RegisterType<VoteService>().AsFactory().WithProduct<IVoteService>().SingleInstance();
            builder.RegisterType<ExceptionReportsService>().AsFactory().WithProduct<IExceptionReportsService>().SingleInstance();
            builder.RegisterType<QuizService>().AsFactory().WithProduct<IQuizService>().SingleInstance();
            builder.RegisterType<QuizQuestionsService>().AsFactory().WithProduct<IQuizQuestionService>().SingleInstance();

            builder.RegisterType<CommandExecutionContext>().As<ICommandExecutionContext>();

            builder.RegisterInstance(client).As<DiscordClient>().As<CustomDiscordClient>().SingleInstance();



            foreach (var type in types)          
                builder.RegisterType(type).SingleInstance();

            foreach (var type in BL.BotBackbone.GetModules())
            {
                if (type.attr.IsChannelContextual)
                    builder.RegisterType(type.type);
                else
                    builder.RegisterType(type.type).SingleInstance();
            }


            _container = builder.Build().BeginLifetimeScope();
        }

        public static ILifetimeScope ObtainScope() => _container.BeginLifetimeScope();

        public static DiscordClient DiscordClient => _container.Resolve<DiscordClient>();

        public static IBotContext BotContext => _container.Resolve<IBotContext>();      
        public static IBotBackbone BotBackbone => _container.Resolve<IBotBackbone>();
        public static IExceptionHandler ExceptionHandler => _container.Resolve<IExceptionHandler>();

        public static ICharacterDetailsQuery CharacterDetailsQuery => _container.Resolve<ICharacterDetailsQuery>();
        public static IProfileQuery ProfileQuery => _container.Resolve<IProfileQuery>();
        public static ICharactersSearchQuery CharactersSearchQuery => _container.Resolve<ICharactersSearchQuery>();
        public static IStaffDetailsQuery StaffDetailsQuery => _container.Resolve<IStaffDetailsQuery>();

        public static IElectionService ElectionService => _container.Resolve<IElectionService>();
        public static IWaifuService WaifuService => _container.Resolve<IWaifuService>();
        public static IUserService UserService => _container.Resolve<IUserService>();
        public static IContenderService ContenderService => _container.Resolve<IContenderService>();

        public static ICommandExecutionContext ExecutionContext => _container.Resolve<ICommandExecutionContext>();
    }
}
