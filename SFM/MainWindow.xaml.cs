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
            viewModel.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(viewModel.CurrentConnections) || e.PropertyName == nameof(viewModel.SelectedDialogue) || e.PropertyName == nameof(viewModel.SelectedConnection))
                {
                RedrawConnections(); // Рисуем линии, когда создана новая связь
                }   
            };
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (DataContext is MainViewModel vm)
                {
                    vm.SelectedNode = null;
                    vm.SelectedConnection = null;

                    RedrawConnections();
                }
            }
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
                if (window.ShowDialog() == true && window.SelectedNpc != null)
                {
                    service.AddDialogue(window.SelectedNpc, window.DialoName);
                    RefreshDialogueList();
                }
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
                var fromNode = vm.SelectedDialogue.Nodes.FirstOrDefault(n => n.Id == conn.FromNodeId);
                var toNode = vm.SelectedDialogue.Nodes.FirstOrDefault(n => n.Id == conn.ToNodeId);
                if (fromNode == null || toNode == null) continue;

                double startX = fromNode.X + 180; // Ширина ноды
                double startY = fromNode.Y + 40;
                double endX = toNode.X;
                double endY = toNode.Y + 40;
                double offset = Math.Abs(endX - startX) * 0.5;

                // Рисуем кривую
                Path path = new Path
                {
                    Stroke = Brushes.MediumPurple,
                    StrokeThickness = 2,
                    Data = new PathGeometry(new[] {
                    new PathFigure(new Point(startX, startY), new[] {
                    new BezierSegment(new Point(startX + offset, startY), new Point(endX - offset, endY), new Point(endX, endY), true)}, false)})};
                ConnectionsCanvas.Children.Add(path);

                var label = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(35, 35, 35)),
                    BorderBrush = Brushes.MediumPurple,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(8, 4, 8, 4),
                    Cursor = Cursors.Hand,
                    Child = new TextBlock { Text = conn.ChoiceText, Foreground = Brushes.White, FontSize = 10 }
                };

                if (vm.SelectedConnection == conn)
                {
                    label.BorderBrush = Brushes.Cyan;
                    label.BorderThickness = new Thickness(2);
                }

                label.MouseDown += (s, e) => {
                    vm.SelectedConnection = conn;
                    vm.SelectedNode = null;
                    e.Handled = true; // Чтобы клик не "провалился" на фон
                };

                // Центрируем плашку
                Canvas.SetLeft(label, (startX + endX) / 2 - 40);
                Canvas.SetTop(label, (startY + endY) / 2 - 15);
                ConnectionsCanvas.Children.Add(label);
            }
        }

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

        //Обработка клика по ноде
        private void Node_PreviewMouseDown(object sender, MouseEventArgs e)
        {
            if(DataContext is MainViewModel vm)
            {
                var element = sender as FrameworkElement;
                var node = element?.DataContext as Node;

                if (node!= null) {
                    vm.SelectedNode = node;
                }
            }
        }
        private Point _lastMousePosition;
        private bool _isPanning;

        private void Editor_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var matrix = EditorMatrixTransform.Matrix;
            var scale = e.Delta > 0 ? 1.1 : 0.9; // Сила зума

            // Точка, куда указывает мышь (центр зума)
            var position = e.GetPosition(EditorContent);
            matrix.ScaleAtPrepend(scale, scale, position.X, position.Y);

            EditorMatrixTransform.Matrix = matrix;
            e.Handled = true; // Чтобы не прокручивались другие элементы
        }

        private void Editor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Если нажали среднюю кнопку или просто на пустой фон холста
            if (e.ChangedButton == MouseButton.Middle || e.Source is Canvas)
            {
                _lastMousePosition = e.GetPosition(this);
                _isPanning = true;
                ((UIElement)sender).CaptureMouse();

                // Сбрасываем выбор ноды, если кликнули по фону
                if (DataContext is MainViewModel vm && e.Source is Canvas)
                {
                    vm.SelectedNode = null;
                    vm.SelectedConnection = null;
                }
            }
        }

        private void Editor_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isPanning)
            {
                Point currentPosition = e.GetPosition(this);
                Vector delta = currentPosition - _lastMousePosition;
                _lastMousePosition = currentPosition;

                var matrix = EditorMatrixTransform.Matrix;
                matrix.Translate(delta.X, delta.Y);
                EditorMatrixTransform.Matrix = matrix;
            }
        }

        private void Editor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isPanning = false;
            ((UIElement)sender).ReleaseMouseCapture();
        }

        
    }
}