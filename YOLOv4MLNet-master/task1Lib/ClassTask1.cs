using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using task1Lib.DataStructures;
using static Microsoft.ML.Transforms.Image.ImageResizingEstimator;
using System.Threading;
using System.Collections.Concurrent;

namespace task1Lib
{
    public class ClassTask1
    {
        public object SelectedItem; //Выбранный пользователем для просмотра всех изображений, связанных с данным типом
        public event Action<ResultInfo> OnProcessedPicture;
        public static CancellationTokenSource cancelTokenSource;
        public static CancellationToken token;

    // model is available here:
    // https://github.com/onnx/models/tree/master/vision/object_detection_segmentation/yolov4
    const string modelPath = @"C:\Users\monul\OneDrive\Desktop\4kurs\YOLOv4MLNet-master\model\yolov4.onnx";
        //const string modelPath = @"C:\Users\monul\OneDrive\Desktop\yolov4\yolov4.onnx";

        //const string imageFolder = @"Assets\Images";
        static SemaphoreSlim sem = new SemaphoreSlim(1);
        static int percent = 0;
        const string imageOutputFolder = @"C:\Users\monul\OneDrive\Desktop\lab\441_samarova\YOLOv4MLNet-master\YOLOv4MLNet\Assets\Output";

        static readonly string[] classesNames = new string[] { "person", "bicycle", "car", "motorbike", "aeroplane", "bus", "train", "truck", "boat", "traffic light", "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow", "elephant", "bear", "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee", "skis", "snowboard", "sports ball", "kite", "baseball bat", "baseball glove", "skateboard", "surfboard", "tennis racket", "bottle", "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple", "sandwich", "orange", "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair", "sofa", "pottedplant", "bed", "diningtable", "toilet", "tvmonitor", "laptop", "mouse", "remote", "keyboard", "cell phone", "microwave", "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase", "scissors", "teddy bear", "hair drier", "toothbrush" };

        public static void ShowProgress(int len)
        {
            sem.Wait();
            try
            {
                Interlocked.Increment(ref percent);
                Console.WriteLine((float)percent / len * 100 + "%");
            }
            finally
            {
                sem.Release();
            }
        }
        //static List<resultInfo> arResult = new List<resultInfo>();
        public async Task RecognizeAsync(string imageFolder, ConcurrentQueue<ResultInfo> arResult, Action<int> showProgress, CancellationToken token)
        {
            /*List<ResultInfo> arResult = new List<ResultInfo>();*/
            Directory.CreateDirectory(imageOutputFolder);
            MLContext mlContext = new MLContext();


            // model is available here:
            // https://github.com/onnx/models/tree/master/vision/object_detection_segmentation/yolov4

            // Define scoring pipeline
            var pipeline = mlContext.Transforms.ResizeImages(inputColumnName: "bitmap", outputColumnName: "input_1:0", imageWidth: 416, imageHeight: 416, resizing: ResizingKind.IsoPad)
                .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input_1:0", scaleImage: 1f / 255f, interleavePixelColors: true))
                .Append(mlContext.Transforms.ApplyOnnxModel(
                    shapeDictionary: new Dictionary<string, int[]>()
                    {
                        { "input_1:0", new[] { 1, 416, 416, 3 } },
                        { "Identity:0", new[] { 1, 52, 52, 3, 85 } },
                        { "Identity_1:0", new[] { 1, 26, 26, 3, 85 } },
                        { "Identity_2:0", new[] { 1, 13, 13, 3, 85 } },
                    },
                    inputColumnNames: new[]
                    {
                        "input_1:0"
                    },
                    outputColumnNames: new[]
                    {
                        "Identity:0",
                        "Identity_1:0",
                        "Identity_2:0"
                    },
                    modelFile: modelPath, recursionLimit: 100));

            // Fit on empty list to obtain input data schema
            var model = pipeline.Fit(mlContext.Data.LoadFromEnumerable(new List<YoloV4BitmapData>()));

            // Create prediction engine

            // save model
            //mlContext.Model.Save(model, predictionEngine.OutputSchema, Path.ChangeExtension(modelPath, "zip"));


            string[] pictures = Directory.GetFiles(imageFolder);

            var ab = new ActionBlock<string>(imageName =>
            {
                if (token.IsCancellationRequested == false)
                {

                    var predictionEngine = mlContext.Model.CreatePredictionEngine<YoloV4BitmapData, YoloV4Prediction>(model);
                    using (var bitmap = new Bitmap(Image.FromFile(imageName)))
                    {
                        var predict = predictionEngine.Predict(new YoloV4BitmapData() { Image = bitmap });
                        var results = predict.GetResults(classesNames, 0.3f, 0.7f);
                        var objClasses = new List<string>();
                        foreach (var res in results)
                        {
                            if (!objClasses.Contains(res.Label))
                            {
                                objClasses.Add(res.Label);
                            }
                        }
                        if (token.IsCancellationRequested == false)
                        {
                            OnProcessedPicture(new ResultInfo(results, objClasses, imageName));
                            arResult.Enqueue(new ResultInfo(results, objClasses, imageName));
                        }
                        showProgress(pictures.Length);
                    }
                }
            },
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 4,
                CancellationToken = token
            });
            Parallel.For(0, pictures.Length, i => ab.Post(pictures[i]));
            ab.Complete();
            await ab.Completion;
        }
    }
}
