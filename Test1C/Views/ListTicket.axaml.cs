using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Test1C.Models;
using Test1C.ViewModels;

namespace Test1C;

public partial class ListTicket : UserControl
{
    public ListTicket(List<Ticket> ListTicket, string title, string desc, string filePath, string pathView)
    {
        InitializeComponent();
        DataContext = new ListTicketViewModel(ListTicket, title, desc, filePath, pathView);
    }
}