using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using Test1C.Models;

namespace Test1C.ViewModels
{
    public class QuestionViewModel : ViewModelBase
    {
        private readonly QuestionModel _model;

        public QuestionModel Model => _model;

        public QuestionViewModel(QuestionModel model)
        {
            _model = model;
        }

        // Прокси-свойства для данных
        public int Id => _model.Id; 
        public int TicketNumber => _model.TicketNumber;
        public int QuestionNumber => _model.QuestionNumber;
        public string QuestionText => _model.QuestionText;
        public List<Answer> Answers => _model.Answers;
        public int CorrectAnswer => _model.CorrectAnswer;
        public string ImagePath => _model.ImagePath;
        // UI-специфичные свойства
        private bool _isVisibleCorrectAnswer;
        public bool IsVisibleCorrectAnswer
        {
            get => _isVisibleCorrectAnswer;
            set => this.RaiseAndSetIfChanged(ref _isVisibleCorrectAnswer, value);
        }

        private string _colorBorder = "Transparent";
        public string ColorBorder
        {
            get => _colorBorder;
            set => this.RaiseAndSetIfChanged(ref _colorBorder, value);
        }
    }
}
