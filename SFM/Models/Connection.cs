using SFM.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFM;

public class Connection:ViewModelBase
{
    public Guid FromNodeId {  get; set; }
    public Guid ToNodeId { get; set; }

    private string _choiceText = "Далее...";
    public string ChoiceText
    {
        get => _choiceText;
        set
        {
            _choiceText = value; OnPropertyChanged();
        }
    }

}
