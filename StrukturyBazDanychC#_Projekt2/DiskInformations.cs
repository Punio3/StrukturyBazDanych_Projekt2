using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrukturyBazDanychC__Projekt2
{
    public class DiskInformations
    {
        public int WriteToDisk { get; set; }
        public int ReadFromDisk { get; set; }


        public DiskInformations()
        {
            WriteToDisk = 0;
            ReadFromDisk = 0;
        }

        public void ResetParameters()
        {
            WriteToDisk = 0;
            ReadFromDisk = 0;
        }

        public void ShowDiskParameters()
        {
            Console.WriteLine("\nWrite: " + WriteToDisk + "\nRead: " + ReadFromDisk + "\nSum: " + (WriteToDisk + ReadFromDisk) + "\n");
        }
    }
}
