using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BobDono.Database;
using BobDono.Entities;
using Microsoft.EntityFrameworkCore;

namespace BobDono.BL.Services
{
    public class ElectionService
    {
        #region Singleton

        private ElectionService()
        {

        }

        public static ElectionService Instance { get; } = new ElectionService();

        #endregion


        public async Task<Election> GetElection(long id)
        {
            using (var db = new BobDatabaseContext())
            {
                return await db.Elections.Include(election => election.Contenders)
                    .Include(election => election.BracketStages)
                    .FirstOrDefaultAsync(election => election.Id == id);
            }
        }

        public async Task<Election> CreateElection(Election election, User user)
        {
            using (var db = new BobDatabaseContext())
            {
                db.Users.Attach(user);
                election.Author = user;
                user.Elections.Add(election);
                await db.SaveChangesAsync();

                return election;
            }
        }
    }
}
