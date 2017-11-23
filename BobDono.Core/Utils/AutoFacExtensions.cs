using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Autofac.Builder;
using BobDono.DataAccess.Services;
using BobDono.Interfaces.Services;

namespace BobDono.Core.Utils
{
    public static class AutoFacExtensions
    {
        public static TReturn TypedResolve<TReturn, TParameter>(this ILifetimeScope scope, TParameter parameter)
        {
            return scope.Resolve<TReturn>(new TypedParameter(typeof(TParameter), parameter));
        }

        static TReturn TypedResolve<TReturn>(this ILifetimeScope scope, params object[] parameter)
        {
            return scope.Resolve<TReturn>(parameter.Select(o => new TypedParameter(o.GetType(), o)));
        }

        public class FactoryDelegate<TLimit, TActivatorData, TRegistrationStyle>
        {
            private readonly IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> _builder;

            public FactoryDelegate(IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder)
            {
                _builder = builder;
            }

            public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> WithProduct<TProduct>() where TProduct : class, IServiceFactory<TProduct>
            {
                return _builder.As<IServiceFactory<TProduct>>();
            }
        }

        public static FactoryDelegate<TLimit,TActivatorData,TRegistrationStyle> AsFactory<TLimit, TActivatorData,TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder)
        {
            return new FactoryDelegate<TLimit, TActivatorData, TRegistrationStyle>(builder);
        }
    }
}
