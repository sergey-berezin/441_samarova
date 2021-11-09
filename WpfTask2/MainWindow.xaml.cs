using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public partial class MainWindow : Window
    {
        ConcurrentQueue<ResultInfo> arResult;
        delegate void pictureHandlerDelegate();
        private Dispatcher dispatcher;
        ConcurrentQueue<ResultInfo> queue;
        public event PropertyChangedEventHandler PropertyChanged;

        ClassTask1 pictureRecognizer;

        public MainWindow()
        {
            InitializeComponent();
            dispatcher = Dispatcher.CurrentDispatcher;
            pictureRecognizer = new ClassTask1();
            queue = new ConcurrentQueue<ResultInfo>();
            pictureRecognizer.OnProcessedPicture += (s) => { queue.Enqueue(s); dispatcher.BeginInvoke(DispatcherPriority.Background, new pictureHandlerDelegate(OnProcessedPictureHandler)); };
           }

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
                pictureRecognizer.RecognizeAsync(imageFolder, arResult, ClassTask1.ShowProgress, ClassTask1.token);
            }
        }
        private void OnProcessedPictureHandler()
        {
            if (queue.TryDequeue(out ResultInfo curItem))
            {
                for(int i = 0; i<curItem.classes.Count; i++)
                {
                    
                    if (ListBoxResultInfo.Items.IndexOf(curItem.classes[i])==-1)
                    {
                        ListBoxResultInfo.Items.Add(curItem.classes[i]);

                    }
                }
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
            string curClass = ListBoxResultInfo.SelectedItem.ToString();

            foreach(var item in curArResult)
            {
                if(item.CompareClasses(curClass))
                    ListBoxPictures.Items.Add(new { Img = new BitmapImage(new Uri(item.imageName))});
            }
        }
    }
}
