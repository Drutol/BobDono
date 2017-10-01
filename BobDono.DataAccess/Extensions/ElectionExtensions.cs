using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BobDono.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BobDono.DataAccess.Extensions
{
    public static class ElectionExtensions
    {
        public static IQueryable<Election> IncludeAll(this DbSet<Election> set)
        {
            return set.Include(election => election.Contenders)
                .Include(election => election.BracketStages)
                .Include(election => election.Author);
        }
    }
}
