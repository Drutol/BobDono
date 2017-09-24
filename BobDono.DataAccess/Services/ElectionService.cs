using System.Threading.Tasks;
using BobDono.DataAccess.Database;
using BobDono.Interfaces;
using BobDono.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BobDono.DataAccess.Services
{
    public class ElectionService : IElectionService
    {
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
