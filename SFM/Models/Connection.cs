using System;
using System.Collections.Generic;
using System.Text;

namespace SFM;

public class Connection
{
    public Guid FromNodeId {  get; set; }
    public Guid ToNodeId { get; set; }

    public string ChoiceText { get; set; } = "";
}
