using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SFM.Models;
using SFM.Services;

namespace SFM
{
    /// <summary>
    /// Логика взаимодействия для AddDialogueButton_Click.xaml
    /// </summary>
    public partial class AddDialogueWindow : Window
    {

        public string DialoName { get; set; } = string.Empty;
        //public List<Node> Nodes { get; set; } = new();
        //public List<Connection> Connections { get; set; } = new();

        public AddDialogueWindow(List<Npc> npcs)
        {
            InitializeComponent();
            ChooseNpcBox.ItemsSource = npcs;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        public Npc? SelectedNpc
        {
            get
            {
                return ChooseNpcBox.SelectedItem as Npc;
            }
        }
        private void CreateDia_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DialogueNameBox.Text))
            {
                MessageBox.Show("Введите имя диалога.");
                return;
            }
            if (SelectedNpc == null)
            {
                MessageBox.Show("Выберите NPC.");
                return;
            }


            DialoName = DialogueNameBox.Text;
            

            DialogResult = true;
        }

    }
}
