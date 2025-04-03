using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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
        private readonly string _title;
        private readonly string _description;
        private readonly List<Ticket> _listTicket;

        public ObservableCollection<QuestionViewModel> Questions { get; }
        public List<int> NumberQuestion { get; }

        private int _selectedNumber;
        public int SelectedNumber
        {
            get => _selectedNumber;
            set => this.RaiseAndSetIfChanged(ref _selectedNumber, value);
        }

        private QuestionViewModel _selectedQuestion;
        public QuestionViewModel SelectedQuestion
        {
            get => _selectedQuestion;
            set => this.RaiseAndSetIfChanged(ref _selectedQuestion, value);
        }

        private string _textResult;
        public string TextResult
        {
            get => _textResult;
            set => this.RaiseAndSetIfChanged(ref _textResult, value);
        }

        public ReactiveCommand<QuestionViewModel, Unit> CheckAnswersCommand { get; }

        public ListQuestionsViewModel(List<Ticket> list, string title, string desc, List<QuestionModel> questions)
        {
            _listTicket = list;
            _description = desc;
            _title = title;

            Questions = new ObservableCollection<QuestionViewModel>(
                questions.Select(q => new QuestionViewModel(q)));

            NumberQuestion = questions.Select(q => q.QuestionNumber).ToList();
            CheckAnswersCommand = ReactiveCommand.Create<QuestionViewModel>(CheckAnswers);

            // Синхронизация выбранного элемента
            this.WhenAnyValue(x => x.SelectedNumber)
                .Where(number => number > 0)
                .Subscribe(number =>
                {
                    SelectedQuestion = Questions.FirstOrDefault(q => q.QuestionNumber == number);
                });

            this.WhenAnyValue(x => x.SelectedQuestion)
                .Where(question => question != null)
                .Subscribe(question =>
                {
                    SelectedNumber = question.QuestionNumber;
                });
        }

        public void GoBack()
        {
            MainWindowViewModel.Instance.PageContent = new ListTicket(_listTicket, _title, _description);
        }

        public void CheckAnswers(QuestionViewModel question)
        {
            if (question == null) return;

            var answerChecked = question.Answers.FirstOrDefault(x => x.IsChecked == true);
            bool isCorrect = question.CorrectAnswer == answerChecked?.Number;

            question.IsVisibleCorrectAnswer = !isCorrect;
            question.ColorBorder = isCorrect ? "#1CE942" : "#FF1E1E";
            TextResult = isCorrect ? "✓ Правильно" : "✗ Неправильно";

            MessageBoxManager.GetMessageBoxStandard("Результат", TextResult, ButtonEnum.Ok).ShowAsync();
        }
    }
}
