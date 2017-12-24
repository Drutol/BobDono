using System;
using System.Collections.Generic;
using System.Text;
using BobDono.DataAccess.Database;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;
using BobDono.Models.Entities;

namespace BobDono.DataAccess.Services
{
    public class HallOfFameMemberService : ServiceBase<HallOfFameMember, IHallOfFameMemberService>, IHallOfFameMemberService
    {
        public HallOfFameMemberService()
        {

        }

        private HallOfFameMemberService(BobDatabaseContext dbContext, bool saveOnDispose) : base(dbContext, saveOnDispose)
        {

        }
    }
}
