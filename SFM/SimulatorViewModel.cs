using SFM.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace SFM
{
    public class SimulatorViewModel:ViewModelBase
    {
        private readonly DialogueGraph _graph;
        private readonly Npc _npc;

        public string NpcName => _npc.Name.ToUpper();
        public string NpcStyle => _npc.DialogueStyle ?? "";

        private Node _currentNode = null!;
        public string CurrentNodeText => _currentNode.Text;

        public ObservableCollection<Connection> AvailableChoices { get; } = new();

        public ICommand SelectChoiceCommand { get; }
        public ICommand ResetCommand { get; }

        public SimulatorViewModel(DialogueGraph graph, Npc npc)
        {
            _graph = graph;
            _npc = npc;

            SelectChoiceCommand = new RelayCommand(obj => {
                if (obj is Connection choice) GoToNode(choice.ToNodeId);
            });

            ResetCommand = new RelayCommand(_ => Reset());

            Reset();
        }

        private void Reset()
        {
            var startNode = _graph.Nodes.FirstOrDefault(n => n.Id == _graph.StartNodeId)
                            ?? _graph.Nodes.FirstOrDefault();

            if (startNode != null) GoToNode(startNode.Id);
        }

        private void GoToNode(Guid nodeId)
        {
            var node = _graph.Nodes.FirstOrDefault(n => n.Id == nodeId);
            if (node != null)
            {
                _currentNode = node;
                OnPropertyChanged(nameof(CurrentNodeText));

                AvailableChoices.Clear();
                var choices = _graph.Connections.Where(c => c.FromNodeId == node.Id);
                foreach (var c in choices) AvailableChoices.Add(c);
            }
        }
    }
}
