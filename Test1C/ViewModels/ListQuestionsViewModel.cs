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
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
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

           
        }

        List<int> numberQuestion;
        public List<int> NumberQuestion { get => numberQuestion; set => this.RaiseAndSetIfChanged(ref numberQuestion, value); }

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
        

        string textResult;
        public string TextResult { get => textResult; set => textResult = value; }

        public void GoBack() {
            MainWindowViewModel.Instance.PageContent = new ListTicket(_listTicket, _title, _description);
        }

        public void CheckAnsvers(QuestionModel quest) {
            

            Ansver ansverChecked = quest.Answers.FirstOrDefault(x => x.IsChecked == true);

            TextResult =  quest.CorrectAnswer == ansverChecked.Number ?  "✓ Правильно" : "✗ Неправильно";
            MessageBoxManager.GetMessageBoxStandard("Сообщение", TextResult, ButtonEnum.Ok).ShowAsync();
        }
      
    }
}
