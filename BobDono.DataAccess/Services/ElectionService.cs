using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BobDono.DataAccess.Database;
using BobDono.DataAccess.Extensions;
using BobDono.Interfaces;
using BobDono.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BobDono.DataAccess.Services
{
    public class ElectionService : IElectionService
    {
        public class ElectionUpdateDelegate : IDelegatedEntityUpdate<Election>
        {
            public ElectionUpdateDelegate(Election election)
            {
                Entity = election;
            }

            public Election Entity { get; }

            public async void Dispose()
            {
                using (var db = new BobDatabaseContext())
                {
                    db.Elections.Attach(Entity);
                    await db.SaveChangesAsync();
                }
            }
        }

        public async Task<Election> GetElection(long id)
        {
            using (var db = new BobDatabaseContext())
            {
                return await db.Elections
                    .IncludeAll()
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

        public void Remove(Election election)
        {
            using (var db = new BobDatabaseContext())
            {
                db.Elections.Remove(election);
                db.SaveChanges();
            }
        }

        public IEnumerable<Election> GetAll()
        {
            using (var db = new BobDatabaseContext())
            {
                return db.Elections.IncludeAll().ToList();
            }
        }

        public async Task<IDelegatedEntityUpdate<Election>> ObtainElectionUpdate(long id)
        {
            return new ElectionUpdateDelegate(await GetElection(id));
        }

        public IDelegatedEntityUpdate<Election> ObtainElectionUpdate(Election election)
        {
            return new ElectionUpdateDelegate(election);
        }

        public void Update(Election election)
        {
            using (var db = new BobDatabaseContext())
            {
                db.Elections.Update(election);
                db.SaveChanges();
            }
        }
    }
}
