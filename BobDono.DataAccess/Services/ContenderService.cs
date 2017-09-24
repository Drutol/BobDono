using System;
using System.Threading.Tasks;
using BobDono.DataAccess.Database;
using BobDono.Interfaces;
using BobDono.Models.Entities;

namespace BobDono.DataAccess.Services
{
    public class ContenderService : IContenderService
    {
        public async Task<WaifuContender> CreateContender(User user, Waifu waifu, Election election,
            string customImage = null)
        {
            using (var db = new BobDatabaseContext())
            {
                var contender = new WaifuContender
                {
                    CustomImageUrl = customImage,
                    Proposer = user,
                    Election = election,
                    Waifu = waifu,
                };
                db.Elections.Attach(election);
                db.Users.Attach(user);
                db.Waifus.Attach(waifu);
                election.Contenders.Add(contender);
                try
                {
                    await db.SaveChangesAsync();
                }
                catch (Exception e)
                {

                }


                return contender;
            }

        }
    }
}
