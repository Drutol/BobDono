﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Models.Entities
{
    public class Bracket : IModelWithRelation
    {
        public long Id { get; set; }
        public BracketStage BracketStage { get; set; }

        public int Number { get; set; }
        public WaifuContender FirstContender { get; set; }
        public WaifuContender SecondContender { get; set; }
        public WaifuContender ThirdContender { get; set; }

        public WaifuContender Winner { get; set; }
        public WaifuContender Loser { get; set; }

        public virtual ICollection<Vote> Votes { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bracket>()
                .HasMany(b => b.Votes)
                .WithOne(v => v.Bracket);
        }

        protected bool Equals(Bracket other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Bracket) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
