using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
//using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using task1Lib;

namespace WpfTask2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ConcurrentQueue<ResultInfo> arResult;

        private Dispatcher dispatcher;
        ConcurrentQueue<ResultInfo> queue;
        public event PropertyChangedEventHandler PropertyChanged;

        ClassTask1 pictureRecognizer;

        public MainWindow()
        {
            //ClassTask1.cancelTokenSource.Cancel();
            
            dispatcher = Dispatcher.CurrentDispatcher;
            pictureRecognizer = new ClassTask1();
            queue = new ConcurrentQueue<ResultInfo>();
            pictureRecognizer.OnProcessedPicture += (s) => { queue.Enqueue(s); dispatcher.Invoke(OnProcessedPictureHandler, DispatcherPriority.Background); };
            InitializeComponent();
            //ListBoxResultInfo.SelectionChanged +=
            /*ListBoxPictures.Items.Add(new { Img = new BitmapImage(new Uri(@"C:\Users\monul\OneDrive\Desktop\lab\441_samarova\YOLOv4MLNet-master\YOLOv4MLNet\Assets\Images\ski.jpg")) });
            ListBoxPictures.Items.Add(new { Img = new BitmapImage(new Uri(@"C:\Users\monul\OneDrive\Desktop\lab\441_samarova\YOLOv4MLNet-master\YOLOv4MLNet\Assets\Images\ski.jpg")) });
            ListBoxPictures.Items.Add(new { Img = new BitmapImage(new Uri(@"C:\Users\monul\OneDrive\Desktop\lab\441_samarova\YOLOv4MLNet-master\YOLOv4MLNet\Assets\Images\ski.jpg")) });
        */}

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ListBoxResultInfo.Items.Clear();
            ClassTask1.cancelTokenSource = new CancellationTokenSource();
            ClassTask1.token = ClassTask1.cancelTokenSource.Token;
            arResult = new ConcurrentQueue<ResultInfo>();
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string imageFolder = fbd.SelectedPath;
                //ListBoxResultInfo.Items.Add(folder);
                pictureRecognizer.RecognizeAsync(imageFolder, arResult, ClassTask1.ShowProgress, ClassTask1.token);
                /*ResultInfo curItem;
                while (arResult.TryDequeue(out curItem))
                {
                    ListBoxResultInfo.Items.Add(curItem.toString());
                }
                ListBoxResultInfo.Items.Add("end");*/
            }
        }
        private void OnProcessedPictureHandler()
        {
            if (queue.TryDequeue(out ResultInfo curItem))
            {
                ListBoxResultInfo.Items.Add(curItem.toString());
                /*pictureLibrary.AddPictureInfo(new PictureInfo(result));*/
                OnPropertyChanged(nameof(ListBoxResultInfo));
            }
        }

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ClassTask1.cancelTokenSource.Cancel();
        }

        private void ListBoxResultInfo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBoxPictures.Items.Clear();
            ResultInfo[] curArResult = arResult.ToArray();
            List<string> curClasses = ListBoxResultInfo.SelectedItem.ToString().Split('\n').ToList();
            curClasses.Sort();
            curClasses.RemoveAt(0);

            foreach(var item in curArResult)
            {
                if(item.CompareClasses(curClasses))
                    ListBoxPictures.Items.Add(new { Img = new BitmapImage(new Uri(item.imageName))});
            }
            //ListBoxResultInfo.Items.Add(curClasses);
            //ListBoxPictures.Items.Add(new { Img = new BitmapImage(new Uri(@"C:\Users\monul\OneDrive\Desktop\lab\441_samarova\YOLOv4MLNet-master\YOLOv4MLNet\Assets\Images\ski.jpg")) });
        }

        /* public void ApplySelection(object selectedItem)
         {
             pictureRecognizer.SelectedItem = selectedItem;
             OnPropertyChanged(nameof(ShowedImages));
         }*/
    }
}
