using System;
using System.Collections.Generic;
using System.Text;

namespace SFM;

public class Npc
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Backstory { get; set; }
    public string? DialogueStyle { get; set; }
    public string? AvatarUrl { get; set; }
    public string? AvatarPresetId { get; set; }//id avatar



}
