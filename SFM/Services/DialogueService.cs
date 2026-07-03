using SFM.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
namespace SFM.Services;
using System.IO;
using System.Linq;
using System.Windows.Shapes;

public class DialogueService
{
    public List<Npc> Npcs { get; private set; } = new();
    public List<DialogueGraph> Dialogues { get; private set; } = new();
    private readonly string _path = "project_data.json";
    public Npc AddNpc(
         string name,
         string description = "",
         string? backstory = null,
         string? dialogueStyle = null,
         string? avatarUrl = null,
         string? avatarPresetId = null)
    {
        var npc = new Npc
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Backstory = backstory,
            DialogueStyle = dialogueStyle,
            AvatarUrl = avatarUrl,
            AvatarPresetId = avatarPresetId
        };

        Npcs.Add(npc);

        return npc;
    }

    public DialogueGraph AddDialogue(Npc npc,string name)
    {
        var dialogue = new DialogueGraph
        {
            Id = Guid.NewGuid(),
            Name = name,
            NpcId = npc.Id
        };

        Dialogues.Add(dialogue);
        return dialogue;
    }
    public Node AddNode(DialogueGraph dialogue, string text)
    {
        var node = new Node
        {
            Id = Guid.NewGuid(),
            Text = text
        };

        dialogue.Nodes.Add(node);
        if (dialogue.StartNodeId == null)
            dialogue.StartNodeId = node.Id;
        return node;
    }
    public Connection AddConnection(DialogueGraph dialogue,Node from,Node to,string choiceText)
    {
        var connection = new Connection
        {
            FromNodeId = from.Id,
            ToNodeId = to.Id,
            ChoiceText = choiceText
        };

        dialogue.Connections.Add(connection);
        return connection;

    }

    public void RenameNpc(Npc npc, string newName) {
        npc.Name = newName;
    }
    public void RenameNode(Node node,string newText)
    {
        node.Text = newText;
    }
    public void RenameDialogue(DialogueGraph dialogueGraph, string newName)
    {
        dialogueGraph.Name = newName;
    }
    public void RemoveNode(DialogueGraph dialogue, Node node)
    {
        dialogue.Nodes.Remove(node);

        dialogue.Connections.RemoveAll(c =>
            c.FromNodeId == node.Id ||
            c.ToNodeId == node.Id);
    }


    public void RemoveNpc(Npc npc)
    {
        Npcs.Remove(npc);
        Dialogues.RemoveAll(d=>d.NpcId == npc.Id);
    }

    public void RemoveDialogue(DialogueGraph dialogueGraph)
    {
        Dialogues.Remove(dialogueGraph);
    }
    public List<DialogueGraph> GetNpcDialogues(Npc npc)
    {
        return Dialogues.Where(d => d.NpcId == npc.Id).ToList();
    }
    public void SaveProject()
    {
        var project = new Project
        {
            Npcs = this.Npcs,
            Dialogues = this.Dialogues
        };

        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(project, options);
        File.WriteAllText(_path, json);
    }

    public void LoadProject(string filePath)
    {
        if (!File.Exists(filePath)) return;

        try
        {
            string json = File.ReadAllText(filePath);
            var project = JsonSerializer.Deserialize<Project>(json);

            if (project != null)
            {
                this.Npcs = project.Npcs ?? new();
                this.Dialogues = project.Dialogues ?? new();
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Ошибка при чтении файла: {ex.Message}");
        }


    }
    
}

