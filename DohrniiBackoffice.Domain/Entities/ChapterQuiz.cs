using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DohrniiBackoffice.Domain.Entities
{
    [Table("ChapterQuiz")]
    public partial class ChapterQuiz
    {
        public ChapterQuiz()
        {
            QuizAnswers = new HashSet<QuizAnswer>();
            QuizAttempts = new HashSet<QuizAttempt>();
        }

        [Key]
        public int Id { get; set; }
        public int ChapterId { get; set; }
        [Unicode(false)]
        public string Question { get; set; } = null!;
        [StringLength(50)]
        [Unicode(false)]
        public string QuestionType { get; set; } = null!;
        [Unicode(false)]
        public string? Tags { get; set; }

        [ForeignKey("ChapterId")]
        [InverseProperty("ChapterQuizs")]
        public virtual Chapter Chapter { get; set; } = null!;
        [InverseProperty("ChapterQuiz")]
        public virtual ICollection<QuizAnswer> QuizAnswers { get; set; }
        [InverseProperty("Quiz")]
        public virtual ICollection<QuizAttempt> QuizAttempts { get; set; }
    }
}
