using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test1C.ViewModels
{
    public class MenuViewModel :ViewModelBase
    {
        public void GoMarathon() {
            MainWindowViewModel.Instance.PageContent = new ListQuestions();
        }
        public void GoTems() {
            MainWindowViewModel.Instance.PageContent = new ListTicket();
        }
        public void GoErrors()
        {
            MainWindowViewModel.Instance.PageContent = new ListTicket();
        }
        public void GoExam()
        {
            MainWindowViewModel.Instance.PageContent = new ListTicket();
        }
    }
}
