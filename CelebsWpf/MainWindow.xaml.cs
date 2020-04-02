using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CelebsWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Busy();
        }

        private void Busy()
        {
            busyGrid.Visibility = Visibility.Visible;
        }

        private void Ready()
        {
            busyGrid.Visibility = Visibility.Collapsed;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => FaceRecognitionUtils.EncodeDataSet("images"));
            Ready();
        }

        private ImageSource GetImageSourceFromPath(string path)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(System.IO.Path.GetFullPath(path));
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }

        private async void searchButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "images(*.jpg)|*.jpg";

            var result = dialog.ShowDialog();
            if (result == true)
            {
                try
                {
                    originalImage.Source = GetImageSourceFromPath(dialog.FileName);
                    similarImage.Source = null;

                    Busy();
                    var matches = await Task.Run(() => FaceRecognitionUtils.Search("images", dialog.FileName));
                    if (matches.Any() == false)
                    {
                        MessageBox.Show("No matches found");
                        return;
                    }

                    var top = matches.First();
                    similarImage.Source = GetImageSourceFromPath(top.ImagePath);
                    resultTextBlock.Text = $"{top.Confidence:p1}";
                    nameTextBlock.Text = top.Name;
                    resultGrid.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    Ready();
                }
            }
        }
    }
}
