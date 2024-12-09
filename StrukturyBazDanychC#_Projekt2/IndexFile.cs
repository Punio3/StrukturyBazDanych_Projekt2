using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace StrukturyBazDanychC__Projekt2
{
    public class IndexFile
    {
        public List<int> Indexes { get; private set; }
        public IndexFile(int amount)
        {
            Indexes = new List<int>(amount);
            for (int i = 0; i < amount; i++)
            {
                Indexes.Add(-1);
            }
        }

        public void ReadIndexes(int amount)
        {
            using (var fs = new FileStream("index.idx", FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fs))
            {
                for (int i = 0; i < amount; i++)
                {
                    try
                    {
                        //odczytanie numeru strony oraz indeksu
                        int pageNumber = reader.ReadInt32();
                        int index = reader.ReadInt32();

                        Indexes[pageNumber] = index;
                    }
                    catch (EndOfStreamException)
                    {
                        Console.WriteLine("Osiągnięto koniec pliku przed wczytaniem wszystkich indeksów.");
                        break;
                    }
                }
            }
        }

        public void WriteIndexes()
        {
            using (var fs = new FileStream("index.idx", FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(fs))
            {
                for (int i = 0; i < Indexes.Count; i++)
                {
                    //zapisanie numeru strony oraz indeksu
                    writer.Write(i);
                    writer.Write(Indexes[i]);
                }
            }
        }
        public void ShowIndexes()
        {
            for (int i = 0; i < Indexes.Count; i++)
            {
                Console.WriteLine("Page Number: " + i + " Key: " + Indexes[i]);
            }
        }
    }
}
