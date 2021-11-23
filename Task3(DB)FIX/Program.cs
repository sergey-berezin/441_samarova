using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task3_DB_FIX
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new PictureLibraryContext())
            {
               
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
