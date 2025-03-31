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

        List<Ticket> listTicket;
        public List<Ticket> ListTicket { get => listTicket; set => this.RaiseAndSetIfChanged( ref listTicket, value); }
        

        Ticket _selectedItem;
        public Ticket SelectedItem { get => _selectedItem; set { this.RaiseAndSetIfChanged(ref _selectedItem, value); GoQuestion(); } }

        List<QuestionModel> _questions;
        public List<QuestionModel> Questions { get => _questions; set => this.RaiseAndSetIfChanged(ref _questions, value); }

        string _title;
        public string Title { get => _title; set => this.RaiseAndSetIfChanged(ref _title , value); }

        string _description;
        public string Description { get => _description; set => this.RaiseAndSetIfChanged(ref _description, value); }

        public ListTicketViewModel(List<Ticket> list,string title, string desc) {
            ListTicket = list;
            Title = title;
            Description = desc;
        }

        public void GoBack() {
            MainWindowViewModel.Instance.PageContent = new Menu();
        }

        public void GoQuestion() {
            Questions = ParseQuestions("File/read1.csv");
            Questions = Questions.Where(x=>x.TicketNumber == SelectedItem.Id).ToList();
            MainWindowViewModel.Instance.PageContent = new ListQuestions(ListTicket, Title, Description, Questions);
        }

        static List<QuestionModel> ParseQuestions(string filePath)
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
                            TicketNumber = int.Parse(record[0]),
                            QuestionNumber = int.Parse(record[1]),
                            CorrectAnswer = int.Parse(record[2]),
                            ImagePath = record[3] == "null" ? null : record[3],
                            QuestionText = record[4].Replace("«", "<").Replace("»", ">"),
                            Answers = new List<string>()
                        };

                        // Добавляем ответы (начиная с 5 поля)
                        for (int i = 5; i < record.Length; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(record[i]))
                            {
                                question.Answers.Add(record[i].Trim());
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
            return questions;
        }

    

    }
}
