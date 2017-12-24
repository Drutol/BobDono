using System;
using System.Collections.Generic;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;
using BobDono.Models.Entities;
using DSharpPlus.EventArgs;

namespace BobDono.Core.BL
{
    public class ExceptionHandler : IExceptionHandler
    {
        private readonly IServiceFactory<IExceptionReportsService> _exceptionReportsService;

        public ExceptionHandler(IServiceFactory<IExceptionReportsService> exceptionReportsService)
        {
            _exceptionReportsService = exceptionReportsService;
        }


        public List<Exception> CaughtThings { get; set; } = new List<Exception>(10);

        public string Handle(Exception e)
        {
            CaughtThings.Add(e);
            

            using (var reportsService = _exceptionReportsService.ObtainLifetimeHandle())
            {
                reportsService.Add(new ExceptionReport
                {
                    Type = e.GetType().Name,
                    Content = e.ToString(),
                    DateTime = DateTime.UtcNow,
                });
            }

            return
                $"Oh no! My paint has spilled all over the place, but don't let our negative emotions get better of us! Try to make up with `{e.GetType().Name}`!";
        }

        public string Handle(Exception e, MessageCreateEventArgs args)
        {
            using (var reportsService = _exceptionReportsService.ObtainLifetimeHandle())
            {
                reportsService.Add(new ExceptionReport
                {
                    Type = e.GetType().Name,
                    Content = e.ToString(),
                    DateTime = DateTime.UtcNow,
                    AffectedUser = args.Message.Author.Username,
                    Channel = args.Channel.Name,
                    Command = args.Message.Content,
                });
            }

            return
                $"Oh no! My paint has spilled all over the place, but don't let our negative emotions get better of us! Try to make up with `{e.GetType().Name}`!";
        }

        public void Handle(Exception e, string comment)
        {
            using (var reportsService = _exceptionReportsService.ObtainLifetimeHandle())
            {
                reportsService.Add(new ExceptionReport
                {
                    Type = e.GetType().Name,
                    Content = $"{comment}\n\n{e}",
                    DateTime = DateTime.UtcNow,                    
                });
            }
        }
    }
}
