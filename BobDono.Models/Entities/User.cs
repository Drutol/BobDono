using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using BobDono.Models.Entities.JoinEntities;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Models.Entities
{
    public class User : IModelWithRelation
    {
        private ICollection<Election> _elections;

        public long Id { get; set; }
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        
        public long _discordId { get; set; }
        [NotMapped]
        public ulong DiscordId
        {
            get { return (ulong) _discordId; }
            set { _discordId = (long) value; }
        }


        public virtual ICollection<Election> Elections => 
            _elections ?? (_elections = new HashSet<Election>());

        public virtual ICollection<UserWaifu> Waifus { get; set; }
        public virtual ICollection<Vote> Votes { get; set; }
        public virtual ICollection<UserTheme> ElectionThemes { get; set; }
        public virtual ICollection<UserMatchup> MatchupsParticipatingIn { get; set; }
        public virtual ICollection<Matchup> CreatedMatchups { get; set; }
        public virtual ICollection<MerchandiseItem> OwnedMerchandiseItems { get; set; }
        public virtual ICollection<QuizSession> QuizSessions { get; set; }

        public TrueWaifu TrueWaifu { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.TrueWaifu)
                .WithOne(w => w.User)
                .HasForeignKey<TrueWaifu>(w => w.UserId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Elections)
                .WithOne(e => e.Author);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Votes)
                .WithOne(vote => vote.User);

            modelBuilder.Entity<User>()
                .HasMany(u => u.CreatedMatchups)
                .WithOne(m => m.Author);

            modelBuilder.Entity<User>()
                .HasMany(u => u.OwnedMerchandiseItems)
                .WithOne(m => m.Owner);
        }
    }
}
