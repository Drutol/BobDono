using System.Linq;
using System.Threading.Tasks;
using BobDono.DataAccess.Database;
using BobDono.Interfaces;
using BobDono.Interfaces.Queries;
using BobDono.Interfaces.Services;
using BobDono.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BobDono.DataAccess.Services
{
    public class WaifuService : ServiceBase<Waifu,IWaifuService> , IWaifuService
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


        public async Task<Waifu> GetOrCreateWaifu(string malId, bool force)
        {
            var waifu = force ? null : await FirstAsync(w => w.MalId == malId);

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

            if (data.VoiceActors.Any())
                waifu.Voiceactors =
                    data.VoiceActors.Take(3).Select(person => $"{person.Name} *({person.Id})*").ToArray();
            if (data.Animeography.Any())
                waifu.Animeography =
                    data.Animeography.Take(3).Select(show => $"{show.Title} *({show.Id})*").ToArray();
            if (data.Mangaography.Any())
                waifu.Mangaography =
                    data.Mangaography.Take(3).Select(manga => $"{manga.Title} *({manga.Id})*").ToArray();

            return waifu;
        }
    }
}
