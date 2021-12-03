using System;

namespace Task3_DB_
{
    class Program
    {
        static void Main(string[] args)
        {
            using(var db = new PictureLibraryContext())
            {
                //Console.WriteLine($"Database path: {db.DbPath}.");
                var a = new PictureInfoDB();
                var b = new PictureTypeDB();
                //db.Add(new Blog { Url = "http://blogs.msdn.com/adonet" });
                //db.SaveChanges();
            }
        }
    }
}
