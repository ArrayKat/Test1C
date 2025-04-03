using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using Test1C.ViewModels;

namespace Test1C.Models
{
    public partial class QuestionModel: ViewModelBase
    {
        public int TicketNumber { get; set; }       // Номер билета (1)
        public int QuestionNumber { get; set; }    // Номер вопроса в билете (1)
        public int CorrectAnswer { get; set; }      // Номер правильного ответа (5)
        public string? ImagePath { get; set; }      // Путь к картинке (null)
        public string QuestionText { get; set; }    // Текст вопроса
        public List<Answer> Answers { get; set; }   // Список ответов

    }
}
