using SFM.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
namespace SFM.Services;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
public class DialogueService
{
    public List<Npc> Npcs { get; private set; } = new();
    public List<DialogueGraph> Dialogues { get; private set; } = new();
    public string? CurrentFilePath { get; set; }
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

    public DialogueGraph AddDialogue(Npc npc, string name)
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
    public Connection AddConnection(DialogueGraph dialogue, Node from, Node to, string choiceText)
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

    public void RenameNpc(Npc npc, string newName)
    {
        npc.Name = newName;
    }
    public void RenameNode(Node node, string newText)
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
        Dialogues.RemoveAll(d => d.NpcId == npc.Id);
    }

    public void RemoveDialogue(DialogueGraph dialogueGraph)
    {
        Dialogues.Remove(dialogueGraph);
    }
    public List<DialogueGraph> GetNpcDialogues(Npc npc)
    {
        return Dialogues.Where(d => d.NpcId == npc.Id).ToList();
    }
    public void SaveProject(string? filePath = null)
    {
        if (filePath != null)
        {
            CurrentFilePath = filePath;
        }

        string targetPath = CurrentFilePath ?? "project_data.json";

        var project = new Project { Npcs = this.Npcs, Dialogues = this.Dialogues };
        var options = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };
        string json = JsonSerializer.Serialize(project, options);

        File.WriteAllText(targetPath, json);
    }

    public void LoadProject(string filePath)
    {
        if (!File.Exists(filePath)) return;

        try
        {
            CurrentFilePath = filePath;

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
    public void ClearProject()
    {
        Npcs.Clear();
        Dialogues.Clear();
        CurrentFilePath = null; // Сбрасываем путь, чтобы "Сохранить" не переписало старый файл
    }

    public void ExportCleanJson(string path)
    {
        var exportData = new
        {
            Npcs = Npcs.Select(n => new { n.Id, n.Name }),
            Dialogues = Dialogues.Select(d => new
            {
                d.Id,
                d.NpcId,
                d.StartNodeId,
                Nodes = d.Nodes.Select(n => new { n.Id, n.Text }),
                Connections = d.Connections.Select(c => new { c.FromNodeId, c.ToNodeId, c.ChoiceText })
            })
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };
        File.WriteAllText(path, JsonSerializer.Serialize(exportData, options));

    }

    public void GenerateCSharpConstants(string path)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("// АВТОГЕНЕРИРУЕМЫЙ ФАЙЛ SFM EDITOR - НЕ РЕДАКТИРОВАТЬ ВРУЧНУЮ");
        sb.AppendLine("namespace SFM.Generated {");

        // Генерируем ID для NPC
        sb.AppendLine("    public static class NpcIds {");
        foreach (var npc in Npcs)
        {
            string safeName = Regex.Replace(npc.Name, @"[^a-zA-Z0-9_]", "_");
            sb.AppendLine($"        public const string {safeName} = \"{npc.Id}\";");
        }
        sb.AppendLine("    }");

        // Генерируем ID для Диалогов
        sb.AppendLine("\n    public static class DialogueIds {");
        foreach (var d in Dialogues)
        {
            string safeName = Regex.Replace(d.Name, @"[^a-zA-Z0-9_]", "_");
            sb.AppendLine($"        public const string {safeName} = \"{d.Id}\";");
        }
        sb.AppendLine("    }");

        sb.AppendLine("}");
        File.WriteAllText(path, sb.ToString());
    }
}
