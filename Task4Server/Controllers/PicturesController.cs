using LibTask1Core;
using LibTask1Core.DataStructures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Task4Server;
using static Microsoft.ML.Transforms.Image.ImageResizingEstimator;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PicturesController : ControllerBase
    {
        public ClassTask1 pictureRecognizer = new ClassTask1();
        private PictureLibraryContext pictureLibraryContext;
        public PicturesController(PictureLibraryContext pictureLibraryContext)
        {
            this.pictureLibraryContext = pictureLibraryContext;
        }

        [HttpPut]
        public List<string> Put(byte[] image) //[FromBody]
        {
            
            byte[] byte_img = image;
            List<string> types = new List<string>();
            MemoryStream ms = new MemoryStream(byte_img);
            var bitmap = Bitmap.FromStream(ms) as Bitmap;
            if (ClassTask1.token.IsCancellationRequested == false)
            {
                ResultInfo result = pictureRecognizer.RecognizeImg(bitmap);
                foreach (var res in result.result)
                {
                    string findRequest = null;
                    Console.WriteLine("Start");
                    var x1 = res.BBox[0];
                    var y1 = res.BBox[1];
                    var x2 = res.BBox[2];
                    var y2 = res.BBox[3];
                    System.Drawing.Rectangle rec = new System.Drawing.Rectangle((int)x1, (int)y1, (int)(x2 - x1), (int)(y2 - y1));
                    Bitmap nb = bitmap.Clone(rec, bitmap.PixelFormat);

                    MemoryStream stream = new MemoryStream();
                    nb.Save(stream, ImageFormat.Bmp);

                    //string findRequest = null;
                    if (ClassTask1.token.IsCancellationRequested == false)
                    {
                        lock (this.pictureLibraryContext)
                            findRequest = this.pictureLibraryContext.FindPicture(stream.ToArray(), Encoding.UTF8.GetBytes(rec.ToString()), res.Label);
                        if (string.IsNullOrEmpty(findRequest))
                        {
                            lock (this.pictureLibraryContext)
                                this.pictureLibraryContext.AddPictureInfo(stream.ToArray(), Encoding.UTF8.GetBytes(rec.ToString()), res.Label);

                        }
                        types.Add(res.Label);
                    }

                }
            }
            return types;
        }

        [HttpDelete]
        public void Delete()
        {
            pictureLibraryContext.ClearDB();
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class InfoController : ControllerBase
    {
        private PictureLibraryContext pictureLibraryContext;

        public InfoController(PictureLibraryContext pictureLibraryContext)
        {
            this.pictureLibraryContext = pictureLibraryContext;
        }
        [HttpGet]
        public string[] Get()
        {
            return pictureLibraryContext.GetAllContent().ToArray();
        }
        /*[HttpPut]
        
        public IEnumerable<string> Put(string type) //[FromBody]
        {
            var result = pictureLibraryContext.GetPicturesByType(type).ToList();
            //return result;
            //return Convert.ToBase64String(result);
            foreach (var image in result)
                yield return Convert.ToBase64String(image);
        }   */
        //public IEnumerable<byte[]> Put(string type) //[FromBody]
/*public List<byte[]> Put(string type) //[FromBody]*/
    }

    [ApiController]
    [Route("api/[controller]")]
    public class TypeController : ControllerBase
    {
        private PictureLibraryContext pictureLibraryContext;

        public TypeController(PictureLibraryContext pictureLibraryContext)
        {
            this.pictureLibraryContext = pictureLibraryContext;
        }

        [HttpGet("{type}")]
        public List<string> Get(string type) //[FromBody]
        {
            List<string> pictures = new List<string>();
            var result = pictureLibraryContext.GetPicturesByType(type).ToList();
            foreach (var image in result)
                pictures.Add(Convert.ToBase64String(image));
            return pictures;
                
        }
    }
}
