using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace SFM.ViewModels;

public class MainViewModel : ViewModelBase
{
    // Список всех NPC
    public ObservableCollection<Npc> Npcs { get; set; } = new();

    private Npc? _selectedNpc;
    public Npc? SelectedNpc
    {
        get => _selectedNpc;
        set { _selectedNpc = value; OnPropertyChanged(); }
    }

    // Список графов (диалогов) для выбранного NPC
    // В моделях нет связи NPC -> Dialogues, добавим её логически здесь
    public ObservableCollection<DialogueGraph> Dialogues { get; set; } = new();

    private DialogueGraph? _selectedDialogue;
    public DialogueGraph? SelectedDialogue
    {
        get => _selectedDialogue;
        set
        {
            _selectedDialogue = value;
            OnPropertyChanged();
            // При смене диалога обновляем списки узлов и связей
            OnPropertyChanged(nameof(CurrentNodes));
            OnPropertyChanged(nameof(CurrentConnections));
        }
    }

    public ObservableCollection<Node>? CurrentNodes => SelectedDialogue?.Nodes != null ? new ObservableCollection<Node>(SelectedDialogue.Nodes) : null;
    public ObservableCollection<Connection>? CurrentConnections => SelectedDialogue?.Connections != null ? new ObservableCollection<Connection>(SelectedDialogue.Connections) : null;

    // Команды
    public ICommand AddNpcCommand { get; }
    public ICommand AddDialogueCommand { get; }
    public ICommand AddNodeCommand { get; }

    public MainViewModel()
    {
        AddNpcCommand = new RelayCommand(_ => {
            var npc = new Npc { Id = Guid.NewGuid(), Name = "Новый NPC" };
            Npcs.Add(npc);
            SelectedNpc = npc;
        });

        AddDialogueCommand = new RelayCommand(_ => {
            if (SelectedNpc == null) return;
            var graph = new DialogueGraph { Id = Guid.NewGuid() };
            Dialogues.Add(graph);
            SelectedDialogue = graph;
        }, _ => SelectedNpc != null);

        AddNodeCommand = new RelayCommand(_ => {
            if (SelectedDialogue == null) return;
            var node = new Node { Id = Guid.NewGuid(), Text = "Новый узел" };
            SelectedDialogue.Nodes.Add(node);
            OnPropertyChanged(nameof(CurrentNodes));
        }, _ => SelectedDialogue != null);

        // Тестовые данные
        var testNpc = new Npc { Name = "Торговец" };
        Npcs.Add(testNpc);
        SelectedNpc = testNpc;
    }
}