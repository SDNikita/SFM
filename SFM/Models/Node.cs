using SFM.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFM;

public class Node: ViewModelBase
{
    public Guid Id{ get; set; }
    private string _text = string.Empty;

    public string Text
    {
        get => _text;
        set { _text = value; OnPropertyChanged(); }
    }

    private double _x;
    public double X
    {
        get => _x;
        set { _x = value; OnPropertyChanged(); }
    }

    private double _y;
    public double Y
    {
        get => _y;
        set { _y = value; OnPropertyChanged(); }
    }

}
