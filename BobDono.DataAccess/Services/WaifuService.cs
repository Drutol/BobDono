using System.Threading.Tasks;
using BobDono.DataAccess.Database;
using BobDono.Interfaces;
using BobDono.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BobDono.DataAccess.Services
{

    public class WaifuService : IWaifuService
    {
        private readonly ICharacterDetailsQuery _characterDetailsQuery;

        public WaifuService(ICharacterDetailsQuery characterDetailsQuery)
        {
            _characterDetailsQuery = characterDetailsQuery;
        }

        public async Task<Waifu> GetOrCreateWaifu(string malId)
        {
            using (var db = new BobDatabaseContext())
            {
                var waifu = await db.Waifus.FirstOrDefaultAsync(w => w.MalId == malId);

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

                await db.Waifus.AddAsync(waifu);
                await db.SaveChangesAsync();
                return waifu;
            }
        }
    }
}
