using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using SFM.Models;
using SFM.Services;
using Microsoft.Win32;
using System.IO.Packaging;
using System.IO;
namespace SFM.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly DialogueService _service;

    public ObservableCollection<Npc> Npcs { get; } = new();
    public ICommand SaveCommand { get; }
    public ICommand OpenProjectCommand { get; }
    public ICommand AddConnectionCommand { get; }
    public ICommand SaveAsCommand { get; }
    public ICommand NewProjectCommand { get; }
    public ICommand SetStartNodeCommand { get; }
    public string CurrentFileName
    {
        get
        {
            if (string.IsNullOrEmpty(_service.CurrentFilePath))
                return "Новый проект";

            return System.IO.Path.GetFileName(_service.CurrentFilePath);
        }
    }
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
            SelectedNode = null;
            SelectedConnection = null;
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
            if (value != null) SelectedConnection = null;
        }
    }
    private Node? _targetNodeForConnection;
    public Node? TargetNodeForConnection
    {
        get => _targetNodeForConnection;
        set
        {
            _targetNodeForConnection = value; OnPropertyChanged();
        }
    }

    private string _newConnectionText = "Далее..";
    public string NewConnectionText
    {
        get=> _newConnectionText;
        set
        {
            _newConnectionText= value; OnPropertyChanged();
        }
    }
    private Connection? _selectedConnection;
    public Connection? SelectedConnection
    {
        get => _selectedConnection;
        set
        {
            _selectedConnection = value;
            OnPropertyChanged();
            if (value != null)  SelectedNode = null; 
        }
    }
    public List<Node>? CurrentNodes => SelectedDialogue?.Nodes?.ToList();
    public List<Connection>? CurrentConnections => SelectedDialogue?.Connections;

    public ICommand AddNpcCommand { get; }
    public ICommand AddDialogueCommand { get; }
    public ICommand AddNodeCommand { get; }
    public ICommand RemoveNodeCommand { get; }


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
            string fileName = Path.GetFileName(_service.CurrentFilePath ?? "project_data.json");
            System.Windows.MessageBox.Show($"Изменения сохранены в файл: {fileName}");
        });

        NewProjectCommand = new RelayCommand(_ => {
            var result = System.Windows.MessageBox.Show(
                "Вы уверены, что хотите создать новый проект? Все несохраненные данные будут потеряны.",
                "Новый проект",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                _service.ClearProject();

                Npcs.Clear();
                CurrentNpcDialogues.Clear();

                SelectedNpc = null;
                SelectedDialogue = null;
                SelectedNode = null;
                SelectedConnection = null;

                OnPropertyChanged(nameof(CurrentFileName));

                System.Windows.MessageBox.Show("Создан новый пустой проект.");
            }
        });

        SetStartNodeCommand = new RelayCommand(_ => {
            if (SelectedDialogue != null && SelectedNode != null)
            {
                SelectedDialogue.StartNodeId = SelectedNode.Id;
                OnPropertyChanged(nameof(CurrentNodes));
                System.Windows.MessageBox.Show("Эта реплика теперь стартовая!");
            }
        }, _ => SelectedNode != null);


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

        //AddNodeCommand = new RelayCommand(_ => {
        //    if (SelectedDialogue == null)
        //    {
        //        System.Windows.MessageBox.Show("Сначала выберите или создайте диалог!");
        //        return;
        //    }

        //    var newNode = _service.AddNode(SelectedDialogue, "Новая реплика...");

        //    newNode.X = 100;
        //    newNode.Y = 100;

        //    OnPropertyChanged(nameof(CurrentNodes));
        //}, _ => SelectedDialogue != null);

        AddDialogueCommand = new RelayCommand(_ => {
            if (SelectedNpc == null) return;

            var graph = _service.AddDialogue(SelectedNpc, "Новый диалог");
            CurrentNpcDialogues.Add(graph);
            SelectedDialogue = graph;
        }, _ => SelectedNpc != null); // Кнопка будет неактивна, если NPC не выбран

        RemoveNodeCommand = new RelayCommand(_ => {
            if (SelectedDialogue == null || SelectedNode == null) return;

            var result = System.Windows.MessageBox.Show(
                "Вы уверены, что хотите удалить эту реплику и все её связи?",
                "Удаление",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                // Удаляем через сервис
                _service.RemoveNode(SelectedDialogue, SelectedNode);

                // Обнуляем выбор, чтобы панель свойств скрылась
                SelectedNode = null;

                // Обновляем UI (узлы и линии)
                OnPropertyChanged(nameof(CurrentNodes));
                OnPropertyChanged(nameof(CurrentConnections));
            }
        }, _ => SelectedNode != null);

        SaveAsCommand = new RelayCommand(_ => {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON файлы (*.json)|*.json";

            if (saveFileDialog.ShowDialog() == true)
            {
                _service.SaveProject(saveFileDialog.FileName);
                System.Windows.MessageBox.Show("Файл успешно создан!");
            }
        });



        AddConnectionCommand = new RelayCommand(_ => {
            if (SelectedNode != null && TargetNodeForConnection != null)
            {
                if (SelectedNode.Id == TargetNodeForConnection.Id)
                {
                    System.Windows.MessageBox.Show("Нельзя создать переход из реплики в саму себя!",
                                                   "Ошибка связи",
                                                   System.Windows.MessageBoxButton.OK,
                                                   System.Windows.MessageBoxImage.Warning);
                    return;
                }

                if (SelectedDialogue != null)
                {
                    _service.AddConnection(SelectedDialogue, SelectedNode, TargetNodeForConnection, NewConnectionText);
                }

                NewConnectionText = "Далее...";
                TargetNodeForConnection = null;

                OnPropertyChanged(nameof(CurrentConnections));
            }
        }, _ => SelectedNode != null &&
                TargetNodeForConnection != null &&
                SelectedNode != TargetNodeForConnection);

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