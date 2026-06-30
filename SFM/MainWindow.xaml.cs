//using SFM.Editor;
using SFM.Models;
using SFM.Services;
using System.Text;
using System.Windows;
using System.Windows.Controls;
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

        

    }
}