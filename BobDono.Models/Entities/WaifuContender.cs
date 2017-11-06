using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Models.Entities
{
    public class WaifuContender : IModelWithRelation
    {
        public long Id { get; set; }
        public int SeedNumber { get; set; }

        public User Proposer { get; set; }
        public Waifu Waifu { get; set; }
        public Election Election { get; set; }

        public string CustomImageUrl { get; set; }
        public string FeatureImage { get; set; }

        public bool Lost { get; set; }

        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WaifuContender>()
                .HasMany(wc => wc.Votes)
                .WithOne(v => v.Contender);

            modelBuilder.Entity<WaifuContender>()
                .HasOne(wc => wc.Election)
                .WithMany(e => e.Contenders);
        }

        protected bool Equals(WaifuContender other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((WaifuContender) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
