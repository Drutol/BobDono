using System;
using System.Collections.Generic;
using System.Text;
using BobDono.DataAccess.Database;
using BobDono.Interfaces.Services;
using BobDono.Models.Entities;

namespace BobDono.DataAccess.Services
{
    public class QuizQuestionsService : ServiceBase<QuizQuestion,IQuizQuestionService> , IQuizQuestionService
    {
        public QuizQuestionsService()
        {

        }

        private QuizQuestionsService(BobDatabaseContext dbContext, bool saveOnDispose) : base(dbContext, saveOnDispose)
        {

        }
    }
}
