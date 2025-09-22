using Backy.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;


namespace Backy
{
    /// <summary>
    /// Interaction logic for AddGameWindow.xaml
    /// </summary>
    public partial class AddGameWindow : Window
    {
        public GameData NewGame { get; private set; }

        public AddGameWindow()
        {
            InitializeComponent();
        }

        private void BrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog dialog = new OpenFolderDialog();

            if (dialog.ShowDialog() == true)
            {
                SaveLocationBox.Text = dialog.FolderName;

            }

        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(GameNameBox.Text) || string.IsNullOrWhiteSpace(SaveLocationBox.Text))
            {
                MessageBox.Show("Please enter both fields.");
                return;
            }

            NewGame = new GameData
            {
                id = 0,
                name = GameNameBox.Text,
                saveLocation = SaveLocationBox.Text,
                storeLocation = "",
            };

            DialogResult = true;

        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
