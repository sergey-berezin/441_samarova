using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using LibTask1Core;
using LibTask1Core.DataStructures;
using Task3_DB_;

namespace WpfTask2Core
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Client client;

        ConcurrentQueue<ResultInfo> arResult;
        delegate void pictureHandlerDelegate();
        private Dispatcher dispatcher;
        ConcurrentQueue<ResultInfo> queue;
        ConcurrentQueue<string> queueTypes;
        public event PropertyChangedEventHandler PropertyChanged;

        ClassTask1 pictureRecognizer;

        PictureLibraryContext db = new PictureLibraryContext();

        public MainWindow()
        {
            InitializeComponent();
            dispatcher = Dispatcher.CurrentDispatcher;
            client = new Client();

            pictureRecognizer = new ClassTask1();
            queue = new ConcurrentQueue<ResultInfo>();
            queueTypes = new ConcurrentQueue<String>();
            client.OnProcessedPicture += (s) => { queueTypes.Enqueue(s); dispatcher.BeginInvoke(DispatcherPriority.Background, new pictureHandlerDelegate(OnProcessedTypeHandler)); };
            client.OnServerIsUnreacheble += () => dispatcher.BeginInvoke(() => Warning());
            LoadAllPictures();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string imageFolder = fbd.SelectedPath;
                client.ScanDirectory(imageFolder);
            }
        }
        private void Warning()
        {
            System.Windows.Forms.MessageBox.Show("Server is unreacheble");
        }
        private void OnProcessedPictureHandler()
        {
            if (queue.TryDequeue(out ResultInfo curItem))
            {
                var bitmap = new Bitmap(System.Drawing.Image.FromFile(curItem.imageName));
                IReadOnlyList<YoloV4Result> results = (IReadOnlyList<YoloV4Result>)curItem.result;


                foreach (var res in results)
                {
                    Console.WriteLine("Start");
                    var x1 = res.BBox[0];
                    var y1 = res.BBox[1];
                    var x2 = res.BBox[2];
                    var y2 = res.BBox[3];
                    System.Drawing.Rectangle rec = new System.Drawing.Rectangle((int)x1, (int)y1, (int)(x2 - x1), (int)(y2 - y1));
                    Bitmap nb = bitmap.Clone(rec, bitmap.PixelFormat);
                  
                    MemoryStream stream = new MemoryStream();
                    nb.Save(stream, ImageFormat.Bmp);
                    
                    string findRequest = null;
                    lock (db)
                        findRequest = db.FindPicture(stream.ToArray(), Encoding.UTF8.GetBytes(rec.ToString()), res.Label);
                    if (string.IsNullOrEmpty(findRequest))
                    {
                        db.AddPictureInfo(stream.ToArray(), Encoding.UTF8.GetBytes(rec.ToString()), res.Label);
                        
                    }
                    if (ListBoxResultInfo.Items.IndexOf(res.Label) == -1)
                    {
                        ListBoxResultInfo.Items.Add(res.Label);
                        OnPropertyChanged(nameof(ListBoxResultInfo));
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
            if (ListBoxResultInfo.Items.Count > 0)
            {
                ListBoxPictures.Items.Clear();
                var result = db.GetPicturesByType(ListBoxResultInfo.SelectedItem.ToString()).ToList();
                foreach (var elem in result)
                {

                    byte[] byte_img = elem;
                    MemoryStream ms = new MemoryStream(byte_img);
                    var bmp = Bitmap.FromStream(ms) as Bitmap;
                    var memory = new MemoryStream();
                    bmp.Save(memory, ImageFormat.Png);
                    memory.Position = 0;

                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();

                    ListBoxPictures.Items.Add(new { Img = bitmapImage });

                }
            }
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                client.ClearDB();
                ListBoxResultInfo.Items.Clear();
                ListBoxPictures.Items.Clear();
            }
            catch
            {
                await dispatcher.BeginInvoke(() => Warning());
            }
        }
        private void OnProcessedTypeHandler()
        {
            if (ClassTask1.token.IsCancellationRequested == false)
            {
                if (queueTypes.TryDequeue(out String curType))
                {
                    if (ListBoxResultInfo.Items.IndexOf(curType) == -1)
                    {
                        ListBoxResultInfo.Items.Add(curType);
                        OnPropertyChanged(nameof(ListBoxResultInfo));
                    }
                }
            }
        }
        public async void LoadAllPictures()
        {
            try
            {
                await foreach (var i in client.LoadAllPictures())
                {
                    queueTypes.Enqueue(i);
                    dispatcher.Invoke(OnProcessedTypeHandler, DispatcherPriority.Background);
                }
            }
            catch
            {
                await dispatcher.BeginInvoke(() => Warning());
            }
        }
    }
}
