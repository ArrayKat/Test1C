using Avalonia.Controls;
using ReactiveUI;

namespace Test1C.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        UserControl _pageContent = new Menu();
        public UserControl PageContent { get => _pageContent; set => this.RaiseAndSetIfChanged( ref _pageContent ,value); }


        public static MainWindowViewModel Instance;
        public MainWindowViewModel() { Instance = this; }
    }
}
