﻿using DohrniiBackoffice.Domain.Abstract;
using DohrniiBackoffice.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DohrniiBackoffice.Domain.Contract
{
    public class EFQuizAnswerRepository : GenericRepository<QuizAnswer>, IQuizAnswerRepository
    {
        public EFQuizAnswerRepository(DohrniiBackOfficeContext context) : base(context)
        {

        }
    }
}
