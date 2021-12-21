using LibTask1Core;
using LibTask1Core.DataStructures;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace WpfTask2Core
{
    class Client
    {
        public ClassTask1 pictureRecognizer= new ClassTask1();

        ConcurrentQueue<ResultInfo> arResult;
        public event Action OnServerIsUnreacheble;
        public event Action<string> OnProcessedPicture;
        public event Action<string> OnReadPicture;

        private HttpClient httpClient = new HttpClient();
        public async void GetPicturesByType(string type)
        {
            try
            {
                await httpClient.GetAsync("http://localhost:5000/api/Type");
            }
            catch
            {
                OnServerIsUnreacheble();
                return;
            }
            var c = new StringContent(type);
            c.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var result = await httpClient.GetAsync("http://localhost:5000/api/type/" + type);
            string[] pictures = JsonConvert.DeserializeObject<string[]>(await result.Content.ReadAsStringAsync());
            foreach (string picture in pictures)
                OnReadPicture( picture);
        }
        public async void ScanDirectory(string imageFolder)
        {
            try
            {
                await httpClient.GetAsync("http://localhost:5000/api/Pictures");
            }
            catch
            {
                OnServerIsUnreacheble();
                return;
            }
            ClassTask1.cancelTokenSource = new CancellationTokenSource();
            ClassTask1.token = ClassTask1.cancelTokenSource.Token;
            arResult = new ConcurrentQueue<ResultInfo>();
            string[] pictures = Directory.GetFiles(imageFolder);
            var ab = new ActionBlock<string>(async imageName =>
            {
                if (ClassTask1.token.IsCancellationRequested == false)
                {
                    var bytes = File.ReadAllBytes(imageName as string);
                    var image = Convert.ToBase64String(bytes);
                    var getRequest = JsonConvert.SerializeObject(image);
                    var c = new StringContent(getRequest);
                    c.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    var result = await httpClient.PutAsync("http://localhost:5000/api/Pictures", c);
                    string[] types = JsonConvert.DeserializeObject<string[]>(await result.Content.ReadAsStringAsync());
                    foreach (var type in types)
                    {
                        OnProcessedPicture(type);
                    }
                }
            },
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 4,
            });
            Parallel.For(0, pictures.Length, i => ab.Post(pictures[i]));
            ab.Complete();
            await ab.Completion;
        }

        public async IAsyncEnumerable<string> LoadAllPictures()
        {
            var result = await httpClient.GetStringAsync("http://localhost:5000/api/Info");
            foreach (var i in JsonConvert.DeserializeObject<string[]>(result))
                yield return i;
        }
        public async void ClearDB()
        {
            await httpClient.DeleteAsync("http://localhost:5000/api/Pictures");
        }
    }
}
