using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Models.Entities
{
    public class TrueWaifu : IModelWithRelation
    {
        public long Id { get; set; }

        public User User { get; set; }
        public Waifu Waifu { get; set; }
        public string Description { get; set; }
        public string FeatureImage { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TrueWaifu>().HasOne(w => w.User).WithOne(u => u.TrueWaifu);
        }
    }
}
