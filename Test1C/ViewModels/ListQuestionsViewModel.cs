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
using Avalonia;
using CsvHelper;
using CsvHelper.Configuration;
using DynamicData.Kernel;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using Test1C;
using Test1C.Models;
using Test1C.ViewModels;

namespace Test1C.ViewModels
{
    public class ListQuestionsViewModel : ViewModelBase
    {
        private readonly List<Ticket> _listTicket;
        private readonly string _filePath;
        private readonly string _pathView;
        private string _title;
        private string _description;

        public ObservableCollection<QuestionViewModel> Questions { get; }
        public ObservableCollection<string> NumberQuestion { get; }

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

        public bool IsVisibleDel { get; }
        public ReactiveCommand<QuestionViewModel, Unit> CheckAnswersCommand { get; }
        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        public string Description
        {
            get => _description;
            set => this.RaiseAndSetIfChanged(ref _description, value);
        }

        public ListQuestionsViewModel(List<Ticket> list, string title, string desc,
            List<QuestionModel> questions, string filePath, string path)
        {
            _listTicket = list;
            _filePath = filePath;
            _pathView = path;
            _title = title;
            _description = desc;

            // Инициализация заголовка и описания
            InitializeTitleAndDescription(questions);
            IsVisibleDel = path == "error";

            // Инициализация вопросов
            Questions = new ObservableCollection<QuestionViewModel>(
                questions.Select(q => new QuestionViewModel(q)));

            // Инициализация номеров вопросов
            NumberQuestion = InitializeQuestionNumbers();

            // Настройка синхронизации выбранного элемента
            SetupSelectionSync();

            CheckAnswersCommand = ReactiveCommand.Create<QuestionViewModel>(CheckAnswers);
        }

        private void InitializeTitleAndDescription(List<QuestionModel> questions)
        {
            if (_listTicket == null || !questions.Any()) return;

            var ticket = _listTicket.FirstOrDefault(x => x.Id == questions.First().TicketNumber);
            var teme = ticket?.Title ?? "По всем темам";
            var countQ = questions.Count;
            var countTeme = ticket?.QuestionCount ?? "961 вопрос";

            switch (_pathView)
            {
                case "marathon":
                    Title = "Марафон по всем билетам";
                    Description = "961 вопрос";
                    break;
                case "exam/teme":
                    Title = teme;
                    Description = "14 вопросов";
                    break;
                case "exam/all":
                    Title = "Экзамен по всем билетам";
                    Description = "14 вопросов";
                    break;
                case "error":
                    Title = teme;
                    Description = $"{countQ} вопросов";
                    break;
                case "teme":
                    Title = teme;
                    Description = countTeme;
                    break;
            }
        }

        private ObservableCollection<string> InitializeQuestionNumbers()
        {
            if (_pathView == "marathon")
            {
                return new ObservableCollection<string>(
                    Enumerable.Range(1, Questions.Count).Select(i => i.ToString()));
            }
            else if (_pathView == "exam/all")
            {
                return new ObservableCollection<string>(
                    Questions.OrderBy(x => x.TicketNumber).Select(q => $"{q.TicketNumber}"));
            }
            else
            {
                return new ObservableCollection<string>(
                    Questions.OrderBy(x => x.QuestionNumber).Select(q => $"{q.QuestionNumber}"));
            }
        }

        private void SetupSelectionSync()
        {
            this.WhenAnyValue(x => x.SelectedNumber)
                .Where(number => number > 0)
                .Subscribe(number =>
                {
                    if (_pathView == "marathon" && number <= Questions.Count)
                    {
                        SelectedQuestion = Questions[number - 1];
                    }
                    else if (_pathView == "exam/all")
                    {
                        SelectedQuestion = Questions.FirstOrDefault(q => q.TicketNumber == number);
                    }
                    else
                    {
                        SelectedQuestion = Questions.FirstOrDefault(q => q.QuestionNumber == number);
                    }
                });

            this.WhenAnyValue(x => x.SelectedQuestion)
                .Where(question => question != null)
                .Subscribe(question =>
                {
                    if (_pathView == "marathon")
                    {
                        SelectedNumber = Questions.IndexOf(question) + 1;
                    }
                    else if (_pathView == "exam/all")
                    {
                        SelectedNumber = question.TicketNumber;
                    }
                    else
                    {
                        SelectedNumber = question.QuestionNumber;
                    }
                });
        }
        private void SetupSelectionSync(ObservableCollection<QuestionViewModel> questions, string path)
        {
            this.WhenAnyValue(x => x.SelectedNumber)
                .Where(number => number > 0)
                .Subscribe(number =>
                {
                    SelectedQuestion = path switch
                    {
                        "marathon" when number <= questions.Count => questions[number - 1],
                        "exam/all" => questions.FirstOrDefault(q => q.TicketNumber == number),
                        _ => questions.FirstOrDefault(q => q.QuestionNumber == number)
                    };
                });

            this.WhenAnyValue(x => x.SelectedQuestion)
                .Where(question => question != null)
                .Subscribe(question =>
                {
                    SelectedNumber = path switch
                    {
                        "marathon" => questions.IndexOf(question) + 1,
                        "exam/all" => question.TicketNumber,
                        _ => question.QuestionNumber
                    };
                });
        }

        public void CheckAnswers(QuestionViewModel question)
        {
            if (question == null) return;

            var selectedAnswer = question.Answers.FirstOrDefault(x => x.IsChecked);
            bool isCorrect = question.CorrectAnswer == selectedAnswer?.Number;

            question.IsVisibleCorrectAnswer = !isCorrect; // Всегда показывать правильный ответ после проверки
            question.ColorBorder = isCorrect ? "#1CE942" : "#FF1E1E";
            TextResult = isCorrect ? "✓ Правильно" : "✗ Неправильно";
        }

        // Остальные методы (GoBack, SaveErrorsToCsv и т.д.) остаются без изменений

        public async Task GoBack()
        {

            if (_pathView == null || !_pathView.Contains("exam") || _pathView == "error")
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
                MainWindowViewModel.Instance.PageContent = _pathView == "marathon" ? new Menu() : new ListTicket(_listTicket, _title, _description, _filePath, _pathView);
            }
            else
            {
                int countError = Questions.Where(x => x.ColorBorder == "#FF1E1E").Count();
                int countSuccess = Questions.Where(x => x.ColorBorder == "#1CE942").Count();
                if (countError + countSuccess != 14)
                {
                    var result = await MessageBoxManager.GetMessageBoxStandard("Уведомление", "Вы точно хотите выйти без сохранения?", ButtonEnum.YesNo, Icon.Warning).ShowAsync();
                    if (result == ButtonResult.Yes) {
                        MainWindowViewModel.Instance.PageContent = new Menu();
                    }
                }
                else
                {
                    int total = Questions.Count;
                    int percentage = Convert.ToInt32(countSuccess * 100.0 / total);
                    MessageBoxManager.GetMessageBoxStandard("Уведомление", $"Вы прошли тест на {percentage}% из 100%", ButtonEnum.Ok, Icon.Info).ShowAsync();
                    var ticket = _listTicket.FirstOrDefault(x => x.Id == Questions.First().TicketNumber);

                    if (_pathView == "exam/all") ticket = _listTicket.First();
                    if (ticket != null && ticket.Percent < percentage)
                    {
                        ticket.Percent = percentage;
                        await UpdateTicketsFile(ticket);
                    }

                    MainWindowViewModel.Instance.PageContent = new Menu();

                }
            }
        }


        private async Task UpdateTicketsFile(Ticket updatedTicket)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "File", "Tems.txt");

            try
            {
                // Читаем все строки файла
                var lines = await File.ReadAllLinesAsync(filePath);

                // Находим строку для обновления
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith(updatedTicket.Title))
                    {
                        // Разбиваем строку на части
                        var parts = lines[i].Split(';');

                        // Формируем обновленную строку
                        lines[i] = $"{parts[0]};{parts[1]};{updatedTicket.Percent};";
                        break;
                    }
                }

                // Записываем обновленные данные обратно в файл
                await File.WriteAllLinesAsync(filePath, lines);
            }
            catch (Exception ex)
            {
                await MessageBoxManager.GetMessageBoxStandard("Ошибка",
                    $"Не удалось сохранить результаты: {ex.Message}",
                    ButtonEnum.Ok,
                    Icon.Error).ShowAsync();
            }
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
                    .GroupBy(q => new { q.TicketNumber, q.QuestionNumber, q.QuestionText })
                    .Select(g => g.First())
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
                var existingErrorIds = new HashSet<(int Ticket, int Question)>();

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

                            if (parts?.Length >= 3 &&
                                int.TryParse(parts[1], out var ticket) &&
                                int.TryParse(parts[2], out var question))
                            {
                                existingErrorIds.Add((ticket, question));
                            }
                        }
                    }
                }

                // 4. Фильтруем только действительно новые ошибки
                var uniqueNewErrors = newErrors
                    .Where(q => !existingErrorIds.Contains((q.TicketNumber, q.QuestionNumber)))
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
                    HasHeaderRecord = !File.Exists(filePath) || new FileInfo(filePath).Length == 0
                };

                // 6. Записываем новые ошибки в файл
                using (var writer = new StreamWriter(filePath, true, Encoding.UTF8))
                using (var csv = new CsvWriter(writer, config))
                {
                    foreach (var question in uniqueNewErrors.OrderBy(q => q.TicketNumber).ThenBy(q => q.QuestionNumber))
                    {
                        csv.WriteField(question.Id);
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
                            string answerText = answer.TextAns
                                .Replace("\"", "\"\"")
                                .Replace("<", "«")
                                .Replace(">", "»");
                            csv.WriteField(answerText);
                        }

                        await csv.NextRecordAsync();
                    }
                }

                await MessageBoxManager.GetMessageBoxStandard(
                    "Сохранено",
                    $"Добавлено {uniqueNewErrors.Count} новых ошибок.",
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

        public async Task DelQuestions(QuestionViewModel question)
        {
            if (question == null) return;

            var result = await MessageBoxManager.GetMessageBoxStandard(
                "Подтверждение",
                "Хотите удалить этот вопрос из списка ошибок?",
                ButtonEnum.YesNo
            ).ShowAsync();

            if (result != ButtonResult.Yes) return;

            try
            {
                // 1. Удаление из файла ошибок
                string errorsDir = Path.Combine(AppContext.BaseDirectory, "File");
                string filePath = Path.Combine(errorsDir, "errors.csv");

                if (File.Exists(filePath))
                {
                    // Читаем все строки файла
                    var lines = await File.ReadAllLinesAsync(filePath);

                    // Фильтруем строки, оставляя только те, которые не соответствуют удаляемому вопросу
                    var newLines = new List<string>();
                    bool headerExists = lines.Length > 0 && lines[0].StartsWith("Id;");

                    foreach (var line in lines)
                    {
                        // Пропускаем заголовок
                        if (headerExists && line == lines[0])
                        {
                            newLines.Add(line);
                            continue;
                        }

                        var parts = line.Split(';');
                        if (parts.Length >= 3 &&
                            int.TryParse(parts[1], out var ticket) &&
                            int.TryParse(parts[2], out var qNumber))
                        {
                            if (!(ticket == question.TicketNumber && qNumber == question.QuestionNumber))
                            {
                                newLines.Add(line);
                            }
                        }
                        else
                        {
                            // Сохраняем строки, которые не удалось распарсить
                            newLines.Add(line);
                        }
                    }

                    // Перезаписываем файл только если количество строк изменилось
                    if (newLines.Count != lines.Length)
                    {
                        await File.WriteAllLinesAsync(filePath, newLines, Encoding.UTF8);
                    }
                }

                // 2. Удаление из UI-коллекций
                string numberToRemove = _pathView == "marathon"
                    ? $"{question.TicketNumber}.{question.QuestionNumber}"
                    : question.QuestionNumber.ToString();

                NumberQuestion.Remove(numberToRemove);
                Questions.Remove(question);

                // 3. Сброс выбора
                if (SelectedQuestion == question)
                {
                    SelectedQuestion = null;
                    SelectedNumber = 0;
                    TextResult = string.Empty;
                }

                // 4. Возврат в меню, если вопросов не осталось
                if (!Questions.Any())
                {
                    MainWindowViewModel.Instance.PageContent = new Menu();
                }

                await MessageBoxManager.GetMessageBoxStandard(
                    "Успешно",
                    "Вопрос удален из списка ошибок",
                    ButtonEnum.Ok
                ).ShowAsync();
            }
            catch (Exception ex)
            {
                await MessageBoxManager.GetMessageBoxStandard(
                    "Ошибка",
                    $"Не удалось удалить вопрос: {ex.Message}",
                    ButtonEnum.Ok
                ).ShowAsync();
            }
        }

        public async Task DelAllQuestion()
        {
            if (!Questions.Any())
            {
                await MessageBoxManager.GetMessageBoxStandard(
                    "Информация",
                    "Нет вопросов для удаления",
                    ButtonEnum.Ok
                ).ShowAsync();
                return;
            }

            var result = await MessageBoxManager.GetMessageBoxStandard(
                "Подтверждение",
                "Хотите удалить все ошибки этого билета и вернуться в меню?",
                ButtonEnum.YesNo
            ).ShowAsync();

            if (result == ButtonResult.Yes)
            {
                try
                {
                    // Получаем номер текущего билета (берем из первого вопроса)
                    int currentTicketNumber = Questions.First().TicketNumber;

                    // Удаляем вопросы из файла errors.csv
                    string errorsDir = Path.Combine(AppContext.BaseDirectory, "File");
                    string filePath = Path.Combine(errorsDir, "errors.csv");

                    if (File.Exists(filePath))
                    {
                        var lines = await File.ReadAllLinesAsync(filePath);
                        var newLines = lines.Where(line =>
                        {
                            var parts = line.Split(';');
                            if (parts.Length >= 2 &&
                                int.TryParse(parts[1], out var ticket) &&
                                int.TryParse(parts[2], out var qNumber))
                            {
                                return ticket != currentTicketNumber;
                            }
                            return true;
                        }).ToList();

                        await File.WriteAllLinesAsync(filePath, newLines, Encoding.UTF8);
                    }

                    // Очищаем текущие коллекции
                    Questions.Clear();
                    NumberQuestion.Clear();

                    // Возвращаемся в меню
                    MainWindowViewModel.Instance.PageContent = new Menu();

                    await MessageBoxManager.GetMessageBoxStandard(
                        "Успешно",
                        $"Все вопросы билета {currentTicketNumber} удалены",
                        ButtonEnum.Ok
                    ).ShowAsync();
                }
                catch (Exception ex)
                {
                    await MessageBoxManager.GetMessageBoxStandard(
                        "Ошибка",
                        $"Не удалось удалить вопросы: {ex.Message}",
                        ButtonEnum.Ok
                    ).ShowAsync();
                }
            }
        }
    }
}


