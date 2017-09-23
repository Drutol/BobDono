using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BobDono.Database;
using BobDono.Entities;

namespace BobDono.BL.Services
{
    public class ContenderService
    {
        #region Singleton

        private ContenderService()
        {

        }

        public static ContenderService Instance { get; } = new ContenderService();

        #endregion

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
