using System;
using System.Collections.Generic;
using System.Text;
using BobDono.Database;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Entities
{
    public class User : IModelWithRelation
    {
        private ICollection<Election> _elections;

        public long Id { get; set; }
        public string Name { get; set; }
        public ulong DiscordId { get; set; }


        public virtual ICollection<Election> Elections => 
            _elections ?? (_elections = new HashSet<Election>());

        public virtual ICollection<UserWaifu> Waifus { get; set; }
        public virtual ICollection<Vote> Votes { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(u => u.Elections)
                .WithOne(e => e.Author);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Votes)
                .WithOne(vote => vote.User);
        }
    }
}
