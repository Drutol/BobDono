using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BobDono.Core.Attributes;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;
using DSharpPlus.EventArgs;

namespace BobDono.Modules
{
    [Module(Name = "Merch",Description = "Show off your merch!")]
    public class MerchModule
    {
        private readonly IServiceFactory<IMerchService> _merchService;

        public MerchModule(IServiceFactory<IMerchService> merchService)
        {
            _merchService = merchService;
        }

        [CommandHandler]
        public async Task SetMerch(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {

        }

        [CommandHandler]
        public async Task GetMerch(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {

        }
    }
}
