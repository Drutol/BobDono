using System;
using System.Threading.Tasks;
using BobDono.DataAccess.Database;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;
using BobDono.Models.Entities;

namespace BobDono.DataAccess.Services
{
    public class ContenderService : ServiceBase<WaifuContender> , IContenderService
    {

        public ContenderService()
        {
            
        }

        private ContenderService(BobDatabaseContext bobDatabaseContext, bool saveOnDispose) : base(bobDatabaseContext,saveOnDispose)
        {

        }

        public WaifuContender CreateContender(User user, Waifu waifu, Election election,
            string customImage = null)
        {
            var contender = new WaifuContender
            {
                CustomImageUrl = customImage,
                Proposer = user,
                Election = election,
                Waifu = waifu,
            };
            election.Contenders.Add(contender);

            return contender;
        }

        public override IServiceBase<WaifuContender> ObtainLifetimeHandle(ICommandExecutionContext executionContext, bool saveOnDispose = true)
        {
            return new ContenderService(executionContext.Context as BobDatabaseContext, saveOnDispose);
        }
    }
}
