using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using SFM.Models;
using SFM.Services;

namespace SFM.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly DialogueService _service;

    public ObservableCollection<Npc> Npcs { get; } = new();
    public ICommand SaveCommand { get; }
    public ObservableCollection<DialogueGraph> CurrentNpcDialogues { get; } = new();

    private Npc? _selectedNpc;
    public Npc? SelectedNpc
    {
        get => _selectedNpc;
        set
        {
            _selectedNpc = value;
            OnPropertyChanged();
            RefreshDialogues();
        }
    }

    private DialogueGraph? _selectedDialogue;
    public DialogueGraph? SelectedDialogue
    {
        get => _selectedDialogue;
        set
        {
            _selectedDialogue = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CurrentNodes));
            OnPropertyChanged(nameof(CurrentConnections));
        }
    }

    public System.Collections.Generic.List<Node>? CurrentNodes => SelectedDialogue?.Nodes;
    public System.Collections.Generic.List<Connection>? CurrentConnections => SelectedDialogue?.Connections;

    public ICommand AddNpcCommand { get; }
    public ICommand AddDialogueCommand { get; }
    public ICommand AddNodeCommand { get; }

    public MainViewModel(DialogueService service)
    {
        _service = service;

        // Команда добавления NPC через сервис
        AddNpcCommand = new RelayCommand(_ => {
            // В реальности тут можно открыть окно, но для примера:
            var npc = _service.AddNpc("Новый NPC");
            Npcs.Add(npc);
            SelectedNpc = npc;
        });
        SaveCommand = new RelayCommand(_ => {
            _service.SaveProject(); // Вызываем метод сохранения из сервиса
            System.Windows.MessageBox.Show("Проект успешно сохранен в JSON!");
        });

        AddDialogueCommand = new RelayCommand(_ => {
            if (SelectedNpc == null) return;

            var graph = _service.AddDialogue(SelectedNpc, "Новый диалог");
            CurrentNpcDialogues.Add(graph);
            SelectedDialogue = graph;
        }, _ => SelectedNpc != null); // Кнопка будет неактивна, если NPC не выбран

        AddNodeCommand = new RelayCommand(_ => {
            if (SelectedDialogue == null) return;

            _service.AddNode(SelectedDialogue, "Текст нового узла");
            OnPropertyChanged(nameof(CurrentNodes));
        }, _ => SelectedDialogue != null);

        foreach (var npc in _service.Npcs)
            Npcs.Add(npc);
    }

    private void RefreshDialogues()
    {
        CurrentNpcDialogues.Clear();
        if (SelectedNpc != null)
        {
            var dialogues = _service.GetNpcDialogues(SelectedNpc);
            foreach (var d in dialogues)
            {
                CurrentNpcDialogues.Add(d);
            }
        }
        SelectedDialogue = CurrentNpcDialogues.FirstOrDefault();
    }

}