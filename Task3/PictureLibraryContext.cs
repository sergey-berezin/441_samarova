using Microsoft.Azure.Amqp.Framing;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task3
{
    public class PictureInfoDB
    {
        public int Id { get; set; }

        public byte[] image { get; set; }
        public byte[] rectangle { get; set; }

        public PictureTypeDB Type { get; set; }

        /*        public PictureInfoDetails PictureInfoDetails { get; set; }
        */
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
        //public string DbPath { get; private set; }
        //protected override void OnConfiguring(DbContextOptionsBuilder o) => o.UseSqlite("Data Source=PictureLibrary.db");

        /* protected override void OnModelCreating(DbModelBuilder modelBuilder)
         {
             modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
             base.OnModelCreating(modelBuilder);
         }*/
        /*public PictureLibraryContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            //DbPath = $"{path}{System.IO.Path.DirectorySeparatorChar}PictureLibrary.db";
        }*/
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
            p.Type = new PictureTypeDB();

            var query = Types.Where(x => transfer.TypeName == x.TypeName); //узнаем, есть ли такой тип в таблице Types
            if (query.Count() > 0)
                p.Type = query.First();
            else
            {
                p.Type = new PictureTypeDB();
                p.Type.TypeName = transfer.TypeName;
                Types.Add(p.Type);
            }
            p.image = (byte[])transfer.image;
            p.rectangle = transfer.rectangle;
            //p.HashCode = transfer.DataToBase64.GetHashCode();
            //Details.Add(p.PictureInfoDetails);
            Pictures.Add(p);
            SaveChanges();
        }
    }
}
