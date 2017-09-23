using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BobDono.Database;
using BobDono.Entities;
using BobDono.Utils;
using Microsoft.EntityFrameworkCore;

namespace BobDono.BL.Services
{
    public class WaifuService
    {
        #region Singleton

        private WaifuService()
        {

        }

        public static WaifuService Instance { get; } = new WaifuService();

        #endregion

        public async Task<Waifu> GetOrCreateWaifu(string malId)
        {
            using (var db = new BobDatabaseContext())
            {
                var waifu = await db.Waifus.FirstOrDefaultAsync(w => w.MalId == malId);

                if (waifu != null)
                    return waifu;

                var data = await BotContext.CharacterDetailsQuery.GetCharacterDetails(int.Parse(malId));

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
