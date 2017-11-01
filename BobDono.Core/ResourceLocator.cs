using System;
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
using BobDono.MalHell.Comm;
using BobDono.MalHell.Queries;
using DSharpPlus;
using Microsoft.Practices.ServiceLocation;

namespace BobDono.Core
{
    public class ResourceLocator 
    {
        private static ILifetimeScope _container;


        public static void RegisterDependencies(DiscordClient client, params Type[] types)
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<BotContext>().As<IBotContext>().InstancePerLifetimeScope();
            builder.RegisterType<BotContext>().As<IBotContext>().InstancePerLifetimeScope();
            builder.RegisterType<BotBackbone>().As<IBotBackbone>().InstancePerLifetimeScope();
            builder.RegisterType<ExceptionHandler>().As<IExceptionHandler>().InstancePerLifetimeScope();
            builder.RegisterType<HttpClientProvider>().As<IHttpClientProvider>().InstancePerLifetimeScope();
            builder.RegisterType<ElectionService>().As<IElectionService>().InstancePerLifetimeScope();
            builder.RegisterType<WaifuService>().As<IWaifuService>().InstancePerLifetimeScope();
            builder.RegisterType<UserService>().As<IUserService>().InstancePerLifetimeScope();
            builder.RegisterType<ContenderService>().As<IContenderService>().InstancePerLifetimeScope();
            builder.RegisterType<CharacterDetailsQuery>().As<ICharacterDetailsQuery>().InstancePerLifetimeScope();
            builder.RegisterType<ProfileQuery>().As<IProfileQuery>().InstancePerLifetimeScope();
            builder.RegisterType<CharactersSearchQuery>().As<ICharactersSearchQuery>().InstancePerLifetimeScope();
            builder.RegisterType<StaffDetailsQuery>().As<IStaffDetailsQuery>().InstancePerLifetimeScope();


            builder.RegisterType<CommandExecutionContext>().As<ICommandExecutionContext>();

            builder.RegisterInstance(client).As<DiscordClient>();



            foreach (var type in types.Union(BL.BotBackbone.GetModules()))
            {
                builder.RegisterType(type);
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
