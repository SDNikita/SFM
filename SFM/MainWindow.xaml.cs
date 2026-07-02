//using SFM.Editor;
using SFM.Models;
using SFM.Services;
using SFM.ViewModels;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace SFM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DialogueService service = new DialogueService(); 
        public MainWindow()
        {
            InitializeComponent();

            var viewModel = new MainViewModel(service);
            this.DataContext = viewModel;
        }

        private void AddNpcButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddNpcWindow();
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                service.AddNpc(
                    window.NpcName,
                    window.NpcDescription ?? "",
                    window.NpcBackstory,
                    window.NpcStyle,
                    window.NpcAvatar
                );

                RefreshNpcList();
            }
        }

        private void AddDialogueButton_Click( object sender, RoutedEventArgs e)
        {
            var window = new AddDialogueWindow(service.Npcs);
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                service.AddDialogue(window.SelectedNpc, window.DialoName);

                RefreshDialogueList();
            }
        }
        private void RefreshNpcList()
        {
            NpcList.ItemsSource = null;
            NpcList.ItemsSource = service.Npcs;
            NpcList.DisplayMemberPath = "Name";
        }

        private void RefreshDialogueList()
        {
            if (NpcList.SelectedItem is Npc selectedNpc)
            {
                DialogueList.ItemsSource = service.GetNpcDialogues(selectedNpc);
                DialogueList.DisplayMemberPath = "Name";
            }
            else
            {
                DialogueList.ItemsSource = null;
            }
        }

        private void NpcList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshDialogueList();
        }

        private void RedrawConnections()
        {
            ConnectionsCanvas.Children.Clear();
            var vm = DataContext as MainViewModel;
            if (vm?.SelectedDialogue == null) return;

            foreach (var conn in vm.SelectedDialogue.Connections)
            {
                // Ищем узлы по ID
                var fromNode = vm.SelectedDialogue.Nodes.FirstOrDefault(n => n.Id == conn.FromNodeId);
                var toNode = vm.SelectedDialogue.Nodes.FirstOrDefault(n => n.Id == conn.ToNodeId);

                if (fromNode == null || toNode == null) continue;

                // Координаты (центр узлов)
                double startX = fromNode.X + 200; // Ширина узла
                double startY = fromNode.Y + 30;
                double endX = toNode.X;
                double endY = toNode.Y + 30;

                double offset = Math.Abs(endX - startX) * 0.5;

                // Рисуем кривую
                Path path = new Path
                {
                    Stroke = Brushes.MediumPurple,
                    StrokeThickness = 2,
                    Data = new PathGeometry(new[] {
                new PathFigure(new Point(startX, startY), new[] {
                    new BezierSegment(
                        new Point(startX + offset, startY),
                        new Point(endX - offset, endY),
                        new Point(endX, endY), true)
                }, false)
            })
                };
                ConnectionsCanvas.Children.Add(path);
            }
        }

        // При движении узла - перерисовываем
        private void Node_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            var node = thumb?.DataContext as Node;
            if (node != null)
            {
                node.X += e.HorizontalChange;
                node.Y += e.VerticalChange;
                RedrawConnections(); // Перерисовываем линии в реальном времени
            }
        }

        private void AddNode_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;

            if (vm?.SelectedDialogue == null)
            {
                MessageBox.Show("Сначала выберите NPC и его диалог!");
                return;
            }

            var newNode = vm.AddNodeToCurrentDialogue("Новая реплика...");

            if (newNode != null)
            {
                newNode.X = 100;
                newNode.Y = 100;

                RedrawConnections();
            }
        }
    }
}