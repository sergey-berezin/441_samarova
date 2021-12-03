using Microsoft.Azure.Amqp.Framing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Task3_DB_
{
    public class PictureInfoDB
    {
        public int Id { get; set; }

        public byte[] image { get; set; }
        public byte[] rectangle { get; set; }

        public PictureTypeDB Type { get; set; }

    }
    public class PictureTypeDB 
    {
        public int Id { get; set; }

        public string TypeName { get; set; }

        public ICollection<PictureInfoDB> Pictures { get; set; }
    }
    public class PictureLibraryContext : DbContext
    {
        public DbSet<PictureInfoDB> Pictures { get; set; }

        public DbSet<PictureTypeDB> Types { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder o) => o.UseSqlite("Data Source=C:\\Users\\monul\\OneDrive\\Desktop\\Новая папка\\441_samarova\\WpfTask2Core\\PictureLibrary.db");
        public PictureLibraryContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
        }
        public string FindPicture(byte[] image, byte[] rectangle, string typeName)
        {
            if (Pictures.Where(p => p.rectangle == rectangle).Count() == 0)
                return null;

            foreach (var p in Pictures.Where(p => p.rectangle == rectangle))
            {
                if (Convert.ToBase64String(p.image) == Convert.ToBase64String(image))
                {
                    return p.Id.ToString();
                }
            }
            return null;
        }
        public void ClearDB()
        {
            foreach (var p in Pictures)
                Pictures.Remove(p);
            foreach (var d in Types)
                Types.Remove(d);

            SaveChanges();
        }
        public void AddPictureInfo(byte[] image, byte[] rectangle, string typeName)
        {
            var p = new PictureInfoDB();
            var t = new PictureTypeDB();
            p.Type = new PictureTypeDB();

            var query = Types.Where(p => typeName == p.TypeName); //узнаем, есть ли такой тип в таблице Types
            if (query.Count() > 0)
                p.Type = query.First();
            else
            {
                p.Type = new PictureTypeDB();
                p.Type.TypeName = typeName;
                Types.Add(p.Type);
            }
            p.image = image;
            p.rectangle = rectangle;
            Pictures.Add(p);
            SaveChanges();
        }
        public IEnumerable<string> GetAllContent()
        {
            foreach (var p in Types)
            {
                yield return p.TypeName;
            }
        }
        public IEnumerable<byte[]> GetPicturesByType(string typeName)
        {
            foreach (var p in Pictures.Where(p => p.Type.TypeName == typeName))
            {
                yield return p.image;
            }

        }
    }
}
