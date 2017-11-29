using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Models.Entities
{
    public class UserTheme : IModelWithRelation
    {      
        public long UserId { get; set; }
        public User User { get; set; }

        public long ThemeId { get; set; }
        public ElectionTheme Theme { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserTheme>()
                .HasKey(pair => new { pair.UserId, pair.ThemeId });

            modelBuilder.Entity<UserTheme>()
                .HasOne(uw => uw.User)
                .WithMany(u => u.ElectionThemes)
                .HasForeignKey(uw => uw.UserId);

            modelBuilder.Entity<UserTheme>()
                .HasOne(uw => uw.Theme)
                .WithMany(u => u.Approvals)
                .HasForeignKey(uw => uw.ThemeId);
        }
    }
}
