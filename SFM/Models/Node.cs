using SFM.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFM;

public class Node: ViewModelBase
{
    public Guid Id{ get; set; }
    public string Text { get; set; } = "";
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
