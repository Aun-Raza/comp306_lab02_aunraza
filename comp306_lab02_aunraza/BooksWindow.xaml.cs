using Amazon.DynamoDBv2.DocumentModel;
using comp306_lab02_aunraza.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace comp306_lab02_aunraza
{
    /// <summary>
    /// Interaction logic for Books.xaml
    /// </summary>
    public partial class BooksWindow : Window
    {
        private readonly User _user;
        public ObservableCollection<Bookshelf> Books = new ObservableCollection<Bookshelf>();

        public BooksWindow(Document userDocument)
        {
            InitializeComponent();
            _user = DynamoServices.ConvertDocumentTo<User>(userDocument);
            booksDatagrid.ItemsSource = Books;
            this.Loaded += async (object sender, RoutedEventArgs e) => 
                await FindBooksByUsername(_user.Username);
        }

        private async Task FindBooksByUsername(string username)
        {
            Books.Clear();
            var bookDocs = await DynamoServices.FindBooksWithUsername(username);
            foreach (var bookDoc in bookDocs)
            {
                var book = DynamoServices.ConvertDocumentTo<Bookshelf>(bookDoc);
                Books.Add(book);
            }

            // Sort Books based on BookMarkTime
            var sortedBooks = Books.OrderByDescending(b => b.BookMarkTime).ToList();

            Books.Clear();
            foreach (var book in sortedBooks)
            {
                Books.Add(book);
            }

        }

        private async void booksDatagrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (booksDatagrid.SelectedItem is null) return;

            var selectedBook = booksDatagrid.SelectedItem as Bookshelf;
            new PDFWindow(selectedBook.Title, selectedBook.BookMarkPage).ShowDialog();
            await FindBooksByUsername(_user.Username);
        }
    }
}
