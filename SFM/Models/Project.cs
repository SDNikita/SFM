using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace SFM.Models;

public class Project
{
    public List<Npc> Npcs { get; set; } = new();
    public List<DialogueGraph> Dialogues { get; set; } = new();
}
