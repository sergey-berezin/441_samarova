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
        ConcurrentQueue<ResultInfo> arResult;
        delegate void pictureHandlerDelegate();
        private Dispatcher dispatcher;
        ConcurrentQueue<ResultInfo> queue;
        public event PropertyChangedEventHandler PropertyChanged;

        ClassTask1 pictureRecognizer;

        PictureLibraryContext db = new PictureLibraryContext();

        public MainWindow()
        {
            InitializeComponent();
            dispatcher = Dispatcher.CurrentDispatcher;
            pictureRecognizer = new ClassTask1();
            queue = new ConcurrentQueue<ResultInfo>();
            pictureRecognizer.OnProcessedPicture += (s) => { queue.Enqueue(s); dispatcher.BeginInvoke(DispatcherPriority.Background, new pictureHandlerDelegate(OnProcessedPictureHandler)); };
            var result = db.GetAllContent().ToList();
            for (int i = 0; i < result.Count; i++)
                ListBoxResultInfo.Items.Add(result[i].TypeName);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ListBoxResultInfo.Items.Clear();
            ClassTask1.cancelTokenSource = new CancellationTokenSource();
            ClassTask1.token = ClassTask1.cancelTokenSource.Token;
            arResult = new ConcurrentQueue<ResultInfo>();
            //Microsoft.Win32.OpenFileDialog fbd = new Microsoft.Win32.OpenFileDialog();
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //if (fbd.ShowDialog() == true)
            {
                string imageFolder = fbd.SelectedPath;
                //string imageFolder = fbd.FileName;
                pictureRecognizer.RecognizeAsync(imageFolder, arResult, ClassTask1.ShowProgress, ClassTask1.token);
            }
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
                    // draw predictions
                    var x1 = res.BBox[0];
                    var y1 = res.BBox[1];
                    var x2 = res.BBox[2];
                    var y2 = res.BBox[3];
                    System.Drawing.Rectangle rec = new System.Drawing.Rectangle((int)x1, (int)y1, (int)(x2 - x1), (int)(y2 - y1));
                    Bitmap nb = bitmap.Clone(rec, bitmap.PixelFormat);
                    //Bitmap nb = new Bitmap(rec.Width, rec.Height, PixelFormat.Format32bppArgb);
                    /*using (var g = Graphics.FromImage(nb))
                    {
                        g.DrawImage(bitmap, -rec.X, -rec.Y);
                    }*/
                    //return nb;
                    Transfer transfer = new Transfer();
                    //Image<Bgr, Byte> img1 = nb.ToImage<Bgr, byte>();
                    //transfer.image = (byte[])nb;
                    MemoryStream stream = new MemoryStream();
                    nb.Save(stream, ImageFormat.Bmp);
                    //return stream.ToArray();
                    
                    //transfer.image = Convert.ToBase64String(Encoding.UTF8.GetBytes(nb.ToString()));
                    transfer.image = Convert.ToBase64String(stream.ToArray());
                    //var recStr = Encoding.UTF8.GetBytes(rec.ToString());
                    transfer.rectangle = Convert.ToBase64String(Encoding.UTF8.GetBytes(rec.ToString()));
                    transfer.TypeName = res.Label;
                    string findRequest = null;
                    lock (db)
                        findRequest = db.FindPicture(transfer);
                    if (string.IsNullOrEmpty(findRequest))
                    {
                        db.AddPictureInfo(transfer);
                        
                    }
                    if (ListBoxResultInfo.Items.IndexOf(res.Label) == -1)
                    {
                        ListBoxResultInfo.Items.Add(res.Label);
                        OnPropertyChanged(nameof(ListBoxResultInfo));
                    }
                }
                

                /* for (int i = 0; i < curItem.classes.Count; i++)
                 {

                     if (ListBoxResultInfo.Items.IndexOf(curItem.classes[i]) == -1)
                     {
                         ListBoxResultInfo.Items.Add(curItem.classes[i]);

                     }
                 }*/
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
            var transfer = new Transfer();
            transfer.TypeName = ListBoxResultInfo.SelectedItem.ToString();
            var result = db.GetPicturesByType(transfer).ToList();
            for (int i = 0; i < result.Count; i++)
            {
                //ListBoxPictures.Items.Add(result[i].image);

                byte[] byte_img = result[i];
                MemoryStream ms = new MemoryStream(byte_img);
                //image.StreamSource = ms;
                var image = Bitmap.FromStream(ms) as Bitmap;
                //Bitmap bmp = (Bitmap)System.Drawing.Image.FromStream(ms);
                //System.Drawing.Image img =Image.FromStream(ms);
                ListBoxPictures.Items.Add(new { Img = image });

                /*byte[] byte_img = Encoding.ASCII.GetBytes(result[i].image);
                MemoryStream ms = new MemoryStream(byte_img);
                //image.StreamSource = ms;
                var image = Bitmap.FromStream(ms) as Bitmap;
                //Bitmap bmp = (Bitmap)System.Drawing.Image.FromStream(ms);
                //System.Drawing.Image img =Image.FromStream(ms);
                ListBoxPictures.Items.Add(new { Img = image });*/
            }
            /*ListBoxPictures.Items.Clear();
            ResultInfo[] curArResult = arResult.ToArray();
            string curClass = ListBoxResultInfo.SelectedItem.ToString();

            foreach (var item in curArResult)
            {
                if (item.CompareClasses(curClass))
                    ListBoxPictures.Items.Add(new { Img = new BitmapImage(new Uri(item.imageName)) });
            }*/
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            db.ClearDB();
            ListBoxResultInfo.Items.Clear();
        }
    }
}
