using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BobDono.DataAccess.Services;
using BobDono.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BobDono.DataAccess.Extensions
{
    public static class ElectionExtensions
    {
        public static IQueryable<Election> IncludeAll(this IQueryable<Election> set)
        {
            return set
                .Include(election => election.Contenders)
                    .ThenInclude(contenders => contenders.Waifu)   
                .Include(election => election.Contenders)
                    .ThenInclude(contenders => contenders.Proposer)             
                .Include(election => election.BracketStages)
                    .ThenInclude(s => s.Brackets)
                    .ThenInclude(bracket => bracket.Votes)
                .Include(election => election.Author);
        }
    }
}
