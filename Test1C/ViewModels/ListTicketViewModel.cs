using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using Test1C.Models;

namespace Test1C.ViewModels
{
    internal class ListTicketViewModel:ViewModelBase
    {
        List<Ticket> listTicket;
        public List<Ticket> ListTicket { get => listTicket; set => this.RaiseAndSetIfChanged( ref listTicket, value); }


        public ListTicketViewModel() {
            ParceFromFile("File/Tems.txt");
        }

        public void GoBack() {
            MainWindowViewModel.Instance.PageContent = new Menu();
        }


        void ParceFromFile(string filePath) {
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
                        Title = parts[0].Trim(),
                        QuestionCount = parts[1].Trim()
                    });
                }
            }

            ListTicket = newList;
        }

    }
}
