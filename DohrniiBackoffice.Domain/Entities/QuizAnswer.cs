using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DohrniiBackoffice.Domain.Entities
{
    [Table("QuizAnswer")]
    public partial class QuizAnswer
    {
        public QuizAnswer()
        {
            QuizAttempts = new HashSet<QuizAttempt>();
        }

        [Key]
        public int Id { get; set; }
        public int ChapterQuizId { get; set; }
        [Unicode(false)]
        public string ChapterOption { get; set; } = null!;
        public bool IsAnswer { get; set; }

        [ForeignKey("ChapterQuizId")]
        [InverseProperty("QuizAnswers")]
        public virtual ChapterQuiz ChapterQuiz { get; set; } = null!;
        [InverseProperty("SelectedQuizAnswer")]
        public virtual ICollection<QuizAttempt> QuizAttempts { get; set; }
    }
}
