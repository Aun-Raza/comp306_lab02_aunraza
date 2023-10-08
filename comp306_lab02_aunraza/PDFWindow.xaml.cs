using Amazon.S3.Model;
using Amazon.S3;
using System;
using System.Collections.Generic;
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
using System.IO;
using System.ComponentModel;

namespace comp306_lab02_aunraza
{
    /// <summary>
    /// Interaction logic for PDFWindow.xaml
    /// </summary>
    public partial class PDFWindow : Window, INotifyPropertyChanged
    {
        private readonly string BUCKET_NAME = "bucket4lab02";
        private readonly string OBJECT_NAME;
        private readonly int PAGE_NUMBER;
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnProperyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
        private Stream _documentStream;

        public Stream DocumentStream
        {
            get { return _documentStream; }
            set 
            { 
                _documentStream = value;
                OnProperyChanged(new PropertyChangedEventArgs("DocumentStream"));
            }
        }



        public PDFWindow(string objectName, int pageNumber)
        {
            PAGE_NUMBER = pageNumber;
            OBJECT_NAME = objectName;
            this.DataContext = this;
            InitializeComponent();
            this.Loaded += async (object sender, RoutedEventArgs e) =>
                await GetObjectFromBucket(BUCKET_NAME, OBJECT_NAME);
        }
        public async Task GetObjectFromBucket(
            string bucketName,
            string objectName
            )
        {
            using (IAmazonS3 s3Client = new AmazonS3Client(Amazon.RegionEndpoint.CACentral1))
            {
                var request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = objectName,
                };

                using (GetObjectResponse response = await s3Client.GetObjectAsync(request))
                {
                    if (response != null)
                    {
                        var memoryStream = new MemoryStream();
                        await response.ResponseStream.CopyToAsync(memoryStream);
                        DocumentStream = memoryStream;
                        pdfDocument.CurrentPage = PAGE_NUMBER;
                    }
                }
            }
        }

        private async void Window_Closed(object sender, EventArgs e)
        {
            int pageNumber = pdfDocument.CurrentPage;
            await DynamoServices.UpdateBookMark(OBJECT_NAME, pageNumber);
        }
    }
}
