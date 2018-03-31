using System;
using System.Collections.Generic;
using System.Text;
using BobDono.DataAccess.Database;
using BobDono.Interfaces.Services;
using BobDono.Models.Entities;

namespace BobDono.DataAccess.Services
{
    public class QuizService : ServiceBase<QuizSession,IQuizService> , IQuizService
    {
        public QuizService()
        {

        }

        private QuizService(BobDatabaseContext dbContext, bool saveOnDispose) : base(dbContext, saveOnDispose)
        {

        }     
    }
}
