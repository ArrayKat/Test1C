using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test1C.Models
{
    public class QuestionModel
    {
        public int TicketNumber { get; set; }       // Номер билета (1)
        public int QuestionNumber { get; set; }    // Номер вопроса в билете (1)
        public int CorrectAnswer { get; set; }      // Номер правильного ответа (5)
        public string? ImagePath { get; set; }      // Путь к картинке (null)
        public string QuestionText { get; set; }    // Текст вопроса
        public List<Ansver> Answers { get; set; }   // Список ответов

       
    }
}
