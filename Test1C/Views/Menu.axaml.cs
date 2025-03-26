using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Test1C.ViewModels;

namespace Test1C;

public partial class Menu : UserControl
{
    public Menu()
    {
        InitializeComponent();
        DataContext = new MenuViewModel();
    }
}