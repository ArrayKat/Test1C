using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using ReactiveUI;
using Test1C.Models;

namespace Test1C.ViewModels
{
    public class ListQuestionsViewModel : ViewModelBase
    {
        string _title;
        string _description;
        List<Ticket> _listTicket;


        List<QuestionModel> _questions;
        public List<QuestionModel> Questions { get => _questions; set => this.RaiseAndSetIfChanged(ref _questions, value); }

        public ListQuestionsViewModel(List<Ticket> list, string title, string desc, List<QuestionModel> question)
        {
            _listTicket = list;
            _description = desc;
            _title = title;

            Questions = question;
            NumberQuestion = Questions.Select(q => q.QuestionNumber).ToList();

            // Синхронизация при изменении SelectedNumber
            this.WhenAnyValue(x => x.SelectedNumber)
                .Where(number => number > 0)
                .Subscribe(number =>
                {
                    SelectedQuestion = Questions.FirstOrDefault(q => q.QuestionNumber == number);
                });

            // Обратная синхронизация при изменении SelectedQuestion
            this.WhenAnyValue(x => x.SelectedQuestion)
                .Where(question => question != null)
                .Subscribe(question =>
                {
                    SelectedNumber = question.QuestionNumber;
                });

            CheckAnswerCommand = ReactiveCommand.Create<QuestionModel>(question =>
            {
                // Здесь логика проверки ответа
                // question - текущий выбранный вопрос
            });
        }

        List<int> numberQuestion;
        public List<int> NumberQuestion { get => numberQuestion; set => numberQuestion = value; }

        // Новые свойства для синхронизации
        private int _selectedNumber;
        public int SelectedNumber
        {
            get => _selectedNumber;
            set => this.RaiseAndSetIfChanged(ref _selectedNumber, value);
        }

        private QuestionModel _selectedQuestion;
        public QuestionModel SelectedQuestion
        {
            get => _selectedQuestion;
            set => this.RaiseAndSetIfChanged(ref _selectedQuestion, value);
        }

        

        

        public void GoBack() {
            MainWindowViewModel.Instance.PageContent = new ListTicket(_listTicket, _title, _description);
        }

        public void checkAnsvers() { 
        
        }
      
    }
}
