using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using SFM.Models;
using SFM.Services;
using Microsoft.Win32;
namespace SFM.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly DialogueService _service;

    public ObservableCollection<Npc> Npcs { get; } = new();
    public ICommand SaveCommand { get; }
    public ICommand OpenProjectCommand { get; }

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
    private Node? _selectedNode;
    public Node? SelectedNode
    {
        get => _selectedNode;
        set
        {
            _selectedNode = value;
            OnPropertyChanged();
        }
    }
    public List<Node>? CurrentNodes => SelectedDialogue?.Nodes?.ToList();
    public List<Connection>? CurrentConnections => SelectedDialogue?.Connections;

    public ICommand AddNpcCommand { get; }
    public ICommand AddDialogueCommand { get; }
    public ICommand AddNodeCommand { get; }

    public MainViewModel(DialogueService service)
    {
        _service = service;

        // Команда добавления NPC через сервис
        AddNpcCommand = new RelayCommand(_ => {
         
            var npc = _service.AddNpc("Новый NPC");
            Npcs.Add(npc);
            SelectedNpc = npc;
        });
        SaveCommand = new RelayCommand(_ => {
            _service.SaveProject(); // Вызываем метод сохранения из сервиса
            System.Windows.MessageBox.Show("Проект успешно сохранен в JSON!");
        });

        OpenProjectCommand = new RelayCommand(_ => {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON файлы (*.json)|*.json|Все файлы (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                // Загружаем данные в сервис
                _service.LoadProject(openFileDialog.FileName);

                //  Обновляем списки в UI
                Npcs.Clear();
                foreach (var npc in _service.Npcs) Npcs.Add(npc);

                SelectedNpc = null; // Сбрасываем выбор
                System.Windows.MessageBox.Show("Проект успешно загружен!");
            }
        });

        AddNodeCommand = new RelayCommand(_ => {
            if (SelectedDialogue == null)
            {
                System.Windows.MessageBox.Show("Сначала выберите или создайте диалог!");
                return;
            }

            var newNode = _service.AddNode(SelectedDialogue, "Новая реплика...");

            newNode.X = 100;
            newNode.Y = 100;

            OnPropertyChanged(nameof(CurrentNodes));
        }, _ => SelectedDialogue != null);

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

    public Node? AddNodeToCurrentDialogue(string text)
    {
        // выбран ли диалог. Без него узел некуда добавлять.
        if (SelectedDialogue == null) return null;

        // 2. Вызываем метод  сервиса
        var newNode = _service.AddNode(SelectedDialogue, text);

        // 3. Устанавливаем начальные координаты (чтобы не в (0,0))
        newNode.X = 100;
        newNode.Y = 100;

        OnPropertyChanged(nameof(CurrentNodes));

        return newNode;
    }
}