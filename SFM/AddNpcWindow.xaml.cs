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

namespace SFM
{
    /// <summary>
    /// Логика взаимодействия для AddNpcWindow.xaml
    /// </summary>
    public partial class AddNpcWindow : Window
    {

        public string NpcName { get; private set; } = "";
        public string NpcDescription { get; private set; } = "";

        public string? NpcBackstory { get; private set; } = "";

        public string? NpcStyle { get; private set; } = "";
        public string? NpcAvatar { get; private set; } = "";



        public AddNpcWindow()
        {
            InitializeComponent();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Введите имя NPC.");
                return;
            }
            if (string.IsNullOrWhiteSpace(DescriptionBox.Text))
            {
                MessageBox.Show("Введите описание NPC.");
                return;
            }

            NpcName = NameBox.Text;
            NpcDescription = DescriptionBox.Text;
            NpcBackstory = BackstoryBox.Text;
            NpcStyle = StyleDialogueBox.Text;
            NpcAvatar = AvatarBox.Text;
                
            DialogResult = true;
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }


}
