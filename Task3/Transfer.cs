using System;
using System.Collections.Generic;
using System.Text;

namespace Task3
{
    /// <summary>
    /// Универсальный пакет для пересылки любых данных 
    /// </summary>
    public class Transfer 
    {
        public byte[] image { get; set; }

        public string TypeName { get; set; }

        public byte[] rectangle { get; set; }
    }
}
