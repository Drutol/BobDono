using System;
using System.Collections.Generic;
using System.Text;
using BobDono.Database;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Entities
{
    public class User : IModelWithRelation
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public ulong DiscordId { get; set; }
        

        public virtual ICollection<Election> Elections { get; set; }
        public virtual ICollection<UserWaifu> Waifus { get; set; }
        public virtual ICollection<Vote> Votes { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(u => u.Elections)
                .WithOne()
                .HasForeignKey(election => election.Id);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Votes)
                .WithOne(vote => vote.User);
        }
    }
}
