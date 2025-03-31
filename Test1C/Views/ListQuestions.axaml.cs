using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Test1C.Models;
using Test1C.ViewModels;

namespace Test1C;

public partial class ListQuestions : UserControl
{
    public ListQuestions(List<Ticket> list, string title, string desc, List<QuestionModel> question)
    {
        InitializeComponent();
        DataContext = new ListQuestionsViewModel(list, title, desc, question);
    }
}