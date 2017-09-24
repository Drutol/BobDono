using Microsoft.EntityFrameworkCore;

namespace BobDono.Models.Entities
{
    public class UserWaifu : IModelWithRelation
    {
        public long UserId { get; set; }
        public User User { get; set; }

        public long WaifuId { get; set; }
        public Waifu Waifu { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserWaifu>()
                .HasKey(pair => new {pair.UserId, pair.WaifuId});

            modelBuilder.Entity<UserWaifu>()
                .HasOne(uw => uw.User)
                .WithMany(u => u.Waifus)
                .HasForeignKey(uw => uw.UserId);

            modelBuilder.Entity<UserWaifu>()
                .HasOne(uw => uw.Waifu)
                .WithMany(u => u.Users)
                .HasForeignKey(uw => uw.WaifuId);
        }
    }
}
