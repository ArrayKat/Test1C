using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Test1C.ViewModels;

namespace Test1C;

public partial class ListTicket : UserControl
{
    public ListTicket()
    {
        InitializeComponent();
        DataContext = new ListTicketViewModel();
    }
}