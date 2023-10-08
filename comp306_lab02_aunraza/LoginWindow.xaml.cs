using Amazon.DynamoDBv2.DocumentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace comp306_lab02_aunraza
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string _username = "aunraza";
        private string _password = "password";
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
        }

        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }


        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public async void Login(object sender, RoutedEventArgs e)
        {
            
            Document userDocument = await DynamoServices.FindUserByUsernameAndPassword(Username, Password);
            Username = "";
            Password = "";
            
            if (userDocument is null)
            {
                MessageBox.Show("Incorrect username or password");
            }
            else
            {
                new BooksWindow(userDocument).ShowDialog();
            }
        }
    }
}
