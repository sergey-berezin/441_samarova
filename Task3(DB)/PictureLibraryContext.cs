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
        protected override void OnConfiguring(DbContextOptionsBuilder o) => o.UseSqlite("Data Source=C:\\Users\\monul\\OneDrive\\Desktop\\Новая папка\\441_samarova\\Task3(DB)\\PictureLibrary.db");
        public PictureLibraryContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
        }
        public string FindPicture(Transfer transfer)
        {
            if (Pictures.Where(p => p.rectangle == Convert.FromBase64String(transfer.rectangle)).Count() == 0)
                return null;

            foreach (var p in Pictures.Where(p => p.rectangle == Convert.FromBase64String(transfer.rectangle)))
            {
                //Entry(p).Reference("image").Load();
                if (Convert.ToBase64String(p.image) == transfer.image)
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
        public void AddPictureInfo(Transfer transfer)
        {
            var p = new PictureInfoDB();
            var t = new PictureTypeDB();
            p.Type = new PictureTypeDB();

            var query = Types.Where(p => transfer.TypeName == p.TypeName); //узнаем, есть ли такой тип в таблице Types
            if (query.Count() > 0)
                p.Type = query.First();
            else
            {
                p.Type = new PictureTypeDB();
                p.Type.TypeName = transfer.TypeName;
                Types.Add(p.Type);
            }
            p.image = Convert.FromBase64String(transfer.image);
            p.rectangle = Convert.FromBase64String(transfer.rectangle);
            //p.HashCode = transfer.DataToBase64.GetHashCode();
            //Details.Add(p.PictureInfoDetails);
            Pictures.Add(p);
            SaveChanges();
        }
        public IEnumerable<Transfer> GetAllContent()
        {
            foreach (var p in Types)
            {
                var transfer = new Transfer();
                transfer.TypeName = p.TypeName;
                yield return transfer;
            }
        }
        public IEnumerable<Transfer> GetPicturesByType(Transfer transfer)
        {
            foreach (var p in Pictures.Where(p => p.Type.TypeName == transfer.TypeName))
            {
                var new_transfer = new Transfer();
                new_transfer.image = Convert.ToBase64String(p.image);
                yield return new_transfer;
            }
            /*foreach (var p in Pictures)
            {
                if(p.Type.TypeName)
                var new_transfer = new Transfer();
                transfer.image = p.image.ToString();
                yield return transfer;
            }*/
        }
    }
}
