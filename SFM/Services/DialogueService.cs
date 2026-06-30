using SFM.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
namespace SFM.Services;
using System.IO;
using System.Linq;
public class DialogueService
{
    public List<Npc> Npcs { get; } = new();
    public List<DialogueGraph> Dialogues { get; } = new();

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
}

