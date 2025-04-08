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
    public class MenuViewModel :ViewModelBase
    {
        List<QuestionModel> _questions;

        List<Ticket> listTicket;
        public List<Ticket> ListTickets { get => listTicket; set => this.RaiseAndSetIfChanged(ref listTicket, value); }


        public void GoMarathon() {
            _questions = ParseQuestions("File/read1.csv");
            MainWindowViewModel.Instance.PageContent = new ListQuestions(null, null, null, _questions, "File/read1.csv", null);
        }
        public void GoTems() {
            ParceFromTeme("File/Tems.txt");
            ListTickets.RemoveAt(0);
            MainWindowViewModel.Instance.PageContent = new ListTicket(ListTickets, "Тренировка по темам", "Ваша цель - все темы должны стать пройденными на 100%", "File/read1.csv", null);
        }
        public void GoErrors()
        {
            ParceFromTeme("File/Tems.txt");
            // 1. Получаем список всех вопросов из файла
            List<QuestionModel> list = ParseQuestions("File/errors.csv");

            // 2. Выбираем уникальные номера билетов
            List<int> uniqueTicketNumbers = list
                .GroupBy(q => q.TicketNumber)
                .Select(g => g.Key)
                .ToList();

          

            // 4. Оставляем только билеты с уникальными номерами
            ListTickets = ListTickets
                .Where(ticket => uniqueTicketNumbers.Contains(ticket.Id)) // предполагая, что ticket.Id соответствует TicketNumber
                .ToList();

            MainWindowViewModel.Instance.PageContent = new ListTicket(ListTickets, "Тренировка по темам", "Ваша цель - все темы должны стать пройденными на 100%", "File/errors.csv", "error");
        }
        public void GoExam()
        {
            ParceFromTeme("File/Tems.txt");
            MainWindowViewModel.Instance.PageContent = new ListTicket(ListTickets, "Экзавмен", "Ваша цель - пройти тест из 14 вопросов", "File/read1.csv", null);
        }


        void ParceFromTeme(string filePath)
        {
            int localId = 0;
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Файл с данными не найден", filePath);
            }

            var newList = new List<Ticket>(); // Создаем временный список

            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(';');
                if (parts.Length >= 2)
                {
                    newList.Add(new Ticket
                    {
                        Id = localId,
                        Title = parts[0].Trim(),
                        QuestionCount = parts[1].Trim()
                    });
                    localId++;
                }
            }

            ListTickets = newList;
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
                            Answers = new List<Answer>()
                        };

                        // Добавляем ответы (начиная с 5 поля)
                        for (int i = 5; i < record.Length; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(record[i]))
                            {
                                Answer tmp = new Answer()
                                {
                                    Number = i - 4,
                                    TextAns = record[i].Trim()
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
            return questions;
        }
    }
}
