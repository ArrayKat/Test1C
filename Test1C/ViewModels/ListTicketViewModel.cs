using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using CsvHelper;
using ReactiveUI;
using Test1C.Models;

namespace Test1C.ViewModels
{
    internal class ListTicketViewModel:ViewModelBase
    {
        string _filePath;
        string _pathView;
        List<Ticket> listTicket;
        public List<Ticket> ListTicket { get => listTicket; set => this.RaiseAndSetIfChanged( ref listTicket, value); }
        

        Ticket _selectedItem;
        public Ticket SelectedItem { get => _selectedItem; set { this.RaiseAndSetIfChanged(ref _selectedItem, value); GoQuestion(); } }

        List<QuestionModel> questions;
        public List<QuestionModel> Questions { get => questions; set => this.RaiseAndSetIfChanged(ref questions, value); }

        string title;
        public string Title { get => title; set => this.RaiseAndSetIfChanged(ref title , value); }

        string description;
        public string Description { get => description; set => this.RaiseAndSetIfChanged(ref description, value); }
        

        bool isVisiblePercent;
        public bool IsVisiblePercent { get => isVisiblePercent; set => this.RaiseAndSetIfChanged(ref isVisiblePercent, value); }

        public ListTicketViewModel(List<Ticket> list,string title, string desc, string filePath, string pathView) {
            ListTicket = list;
            Title = title;
            Description = desc;
            _filePath = filePath;
            _pathView = pathView;

            IsVisiblePercent = (_pathView != null && _pathView.Contains("exam")) ? true : false;
        }

        public void GoBack() {
            MainWindowViewModel.Instance.PageContent = new Menu();
        }

        public void GoQuestion() {
            if (_pathView == null) {
               
                Questions = ParseQuestionsTicket(_filePath, SelectedItem.Id);
                MainWindowViewModel.Instance.PageContent = new ListQuestions(ListTicket, Title, Description, Questions, _filePath, _pathView);
            }
            else if (_pathView.Contains("exam"))
            {
               
                var random = new Random();

                // Получаем все вопросы
                var allQuestions = SelectedItem.Title.Contains("ВСЕ ТЕМЫ") ? ParseQuestionsTicket(_filePath, 0) : ParseQuestionsTicket(_filePath, SelectedItem.Id);
                var selectedQuestions = new List<QuestionModel>();
                if (SelectedItem.Title.Contains("ВСЕ ТЕМЫ"))
                {

                    // Группируем вопросы по номеру билета
                    var groupedQuestions = allQuestions
                        .GroupBy(q => q.TicketNumber)
                        .ToList();

                    // Выбираем по одному случайному вопросу из каждого билета

                    foreach (var group in groupedQuestions)
                    {
                        var questionFromGroup = group.OrderBy(_ => random.Next()).First();
                        selectedQuestions.Add(questionFromGroup);
                    }
                    _pathView = "exam/all";
                }
                else
                {
                    _pathView = "exam/teme";
                    selectedQuestions = allQuestions;
                }
                Questions = selectedQuestions.OrderBy(x => random.Next()).Take(14).ToList();
                MainWindowViewModel.Instance.PageContent = new ListQuestions(ListTicket, Title, Description, Questions, _filePath, _pathView);
            }
            else
            {
                IsVisiblePercent = false;
                Questions = ParseQuestionsTicket(_filePath, SelectedItem.Id);
                MainWindowViewModel.Instance.PageContent = new ListQuestions(ListTicket, Title, Description, Questions, _filePath, _pathView);
            }
        }


        static List<QuestionModel> ParseQuestionsTicket(string filePath, int idTicket)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = false,
                BadDataFound = null,
                Mode = CsvMode.RFC4180,
                Quote = '"',
                TrimOptions = TrimOptions.Trim
            };

            var questions = new List<QuestionModel>();

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                while (csv.Read())
                {
                    try
                    {
                        var record = csv.Parser.Record;
                        if (record == null || record.Length < 5) continue;

                        var question = new QuestionModel
                        {
                            Id = int.Parse(record[0]),
                            TicketNumber = int.Parse(record[1]),
                            QuestionNumber = int.Parse(record[2]),
                            CorrectAnswer = int.Parse(record[3]),
                            ImagePath = record[4] == "null" ? null : record[4],
                            QuestionText = record[5].Replace("«", "<").Replace("»", ">"),
                            Answers = new List<Answer>()
                        };

                        // Добавляем ответы (начиная с 5 поля)
                        for (int i = 6; i < record.Length; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(record[i]))
                            {
                                Answer tmp = new Answer()
                                {
                                    Number = i - 5,
                                    TextAns = record[i].Trim(),
                                    QuestionGroupe = $"{record[0]}-{record[1]}",
                                    IsChecked = false
                                };
                                question.Answers.Add(tmp);
                            }
                        }

                        questions.Add(question);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка обработки строки {csv.Parser.Row}: {ex.Message}");
                    }
                }
            }
            questions = idTicket!=0 ? questions.Where(x => x.TicketNumber == idTicket).ToList() : questions;
            return questions;
        }

       
    }
}
