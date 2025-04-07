using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
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
        private readonly string _filePath;

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

        public ListQuestionsViewModel(List<Ticket> list, string title, string desc, List<QuestionModel> questions, string filePath)
        {
            _listTicket = list;
            _description = desc;
            _title = title;
            _filePath = filePath;
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

        public async Task GoBack()
        {
            List<QuestionViewModel> list = Questions.Where(x => x.ColorBorder == "#FF1E1E").ToList();

            if (list.Any())
            {
                var result = await MessageBoxManager.GetMessageBoxStandard(
                    "Уведомление",
                    "Хотите сохранить вопросы с ошибками?",
                    ButtonEnum.YesNo
                ).ShowAsync();

                if (result == ButtonResult.Yes)
                {
                    await SaveErrorsToCsv(list);
                }
            }

            MainWindowViewModel.Instance.PageContent = new ListTicket(_listTicket, _title, _description, _filePath);
        }

        private async Task SaveErrorsToCsv(List<QuestionViewModel> errorQuestions)
        {
            const string errorsFileName = "errors.csv";

            try
            {
                // 1. Подготовка пути к файлу
                string errorsDir = Path.Combine(AppContext.BaseDirectory, "File");
                Directory.CreateDirectory(errorsDir);
                string filePath = Path.Combine(errorsDir, errorsFileName);

                // 2. Получаем новые ошибки (с неправильными ответами)
                var newErrors = errorQuestions
                    .Where(q => q.ColorBorder == "#FF1E1E")
                    .ToList();

                if (!newErrors.Any())
                {
                    await MessageBoxManager.GetMessageBoxStandard(
                        "Информация",
                        "Нет новых ошибок для сохранения",
                        ButtonEnum.Ok
                    ).ShowAsync();
                    return;
                }

                // 3. Читаем существующие ошибки из файла
                var existingErrors = new HashSet<(int Ticket, int Question)>();

                if (File.Exists(filePath))
                {
                    using (var reader = new StreamReader(filePath))
                    {
                        // Пропускаем заголовок, если он есть
                        if (new FileInfo(filePath).Length > 0)
                            await reader.ReadLineAsync();

                        while (!reader.EndOfStream)
                        {
                            var line = await reader.ReadLineAsync();
                            var parts = line?.Split(';');

                            if (parts?.Length >= 2 &&
                                int.TryParse(parts[0], out var ticket) &&
                                int.TryParse(parts[1], out var question))
                            {
                                existingErrors.Add((ticket, question));
                            }
                        }
                    }
                }

                // 4. Фильтруем только действительно новые ошибки
                var uniqueNewErrors = newErrors
                    .Where(q => !existingErrors.Contains((q.TicketNumber, q.QuestionNumber)))
                    .ToList();

                if (!uniqueNewErrors.Any())
                {
                    await MessageBoxManager.GetMessageBoxStandard(
                        "Информация",
                        "Все найденные ошибки уже были сохранены ранее",
                        ButtonEnum.Ok
                    ).ShowAsync();
                    return;
                }

                // 5. Настройки CSV
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    Encoding = Encoding.UTF8,
                    HasHeaderRecord = !File.Exists(filePath)
                };

                // 6. Записываем новые ошибки в файл
                using (var writer = new StreamWriter(filePath, true, Encoding.UTF8))
                using (var csv = new CsvWriter(writer, config))
                {
                    foreach (var question in uniqueNewErrors)
                    {
                        csv.WriteField(question.TicketNumber);
                        csv.WriteField(question.QuestionNumber);
                        csv.WriteField(question.CorrectAnswer);
                        csv.WriteField(question.ImagePath ?? "null");

                        string questionText = question.QuestionText
                            .Replace("\"", "\"\"")
                            .Replace("<", "«")
                            .Replace(">", "»");
                        csv.WriteField(questionText);

                        foreach (var answer in question.Answers.OrderBy(a => a.Number))
                        {
                            csv.WriteField(answer.TextAns);
                        }

                        await csv.NextRecordAsync();
                    }
                }

                await MessageBoxManager.GetMessageBoxStandard(
                    "Сохранено",
                    $"Добавлено {uniqueNewErrors.Count} новых ошибок",
                    ButtonEnum.Ok
                ).ShowAsync();
            }
            catch (Exception ex)
            {
                await MessageBoxManager.GetMessageBoxStandard(
                    "Ошибка",
                    $"Не удалось сохранить ошибки: {ex.Message}",
                    ButtonEnum.Ok
                ).ShowAsync();
            }
        }
        public void CheckAnswers(QuestionViewModel question)
        {
            if (question == null) return;

            var answerChecked = question.Answers.FirstOrDefault(x => x.IsChecked == true);
            bool isCorrect = question.CorrectAnswer == answerChecked?.Number;

            question.IsVisibleCorrectAnswer = !isCorrect;
            question.ColorBorder = isCorrect ? "#1CE942" : "#FF1E1E";
            TextResult = isCorrect ? "✓ Правильно" : "✗ Неправильно";
        }
    }
}
