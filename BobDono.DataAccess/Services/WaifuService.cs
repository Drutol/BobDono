using System.Threading.Tasks;
using BobDono.DataAccess.Database;
using BobDono.Interfaces;
using BobDono.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BobDono.DataAccess.Services
{
    public class WaifuService : ServiceBase<Waifu> , IWaifuService
    {
        private readonly ICharacterDetailsQuery _characterDetailsQuery;

        public WaifuService(ICharacterDetailsQuery characterDetailsQuery)
        {
            _characterDetailsQuery = characterDetailsQuery;
        }

        private WaifuService(ICharacterDetailsQuery characterDetailsQuery, BobDatabaseContext dbContext, bool saveOnDispose) : base(dbContext,saveOnDispose)
        {
            _characterDetailsQuery = characterDetailsQuery;
        }

        public async Task<Waifu> GetOrCreateWaifu(string malId)
        {
            var waifu = await Context.Waifus.FirstOrDefaultAsync(w => w.MalId == malId);

            if (waifu != null)
                return waifu;

            var data = await _characterDetailsQuery.GetCharacterDetails(int.Parse(malId));

            waifu = new Waifu
            {
                Description = data.Content,
                ImageUrl = data.ImgUrl,
                Name = data.Name,
                MalId = data.Id.ToString()
            };

            return waifu;
        }

        public override IServiceBase<Waifu> ObtainLifetimeHandle(ICommandExecutionContext executionContext, bool saveOnDispose)
        {
            return new WaifuService(_characterDetailsQuery, executionContext.Context as BobDatabaseContext, saveOnDispose);
        }

    }
}
