using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrukturyBazDanychC__Projekt2
{
    public class OverFlowFile
    {
        public List<Record> OverFlowRecords { get; set; }

        public OverFlowFile(int amount)
        {
            OverFlowRecords = new List<Record>(amount);
        }

        public void ReadOverFlowRecords(string filePath, int amount)
        {

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fs))
            {
                for (int i = 0; i < amount; i++)
                {
                    // Odczyt danych rekordu
                    int key = reader.ReadInt32();
                    string value = new string(reader.ReadChars(30)).TrimEnd();
                    int overflowPointer = reader.ReadInt32();

                    // Tworzenie obiektu Record i dodanie do strony
                    OverFlowRecords.Add(new Record(value, key)
                    {
                        OverFlowPointer_Key = overflowPointer,
                    });
                }
            }
        }

        public void WriteOverFlowToFile(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
            using (var writer = new BinaryWriter(fs))
            {
                //zapisanie rekordow z overflow do pliku
                foreach (var record in OverFlowRecords)
                {
                    if (record != null)
                    {
                        writer.Write(record.Key);
                        writer.Write(record.Value.PadRight(30).ToCharArray());
                        writer.Write(record.OverFlowPointer_Key);
                    }
                }
            }
        }

        public void ShowOverFlowFile()
        {
            Console.WriteLine("OverFlowFile: ");
            for (int k = 0; k < OverFlowRecords.Count; k++)
            {
                Console.WriteLine(k+"   Key: " + OverFlowRecords[k].Key + " Value: " + OverFlowRecords[k].Value
                    + " OverFlowPointer: " + OverFlowRecords[k].OverFlowPointer_Key);
            }
            Console.WriteLine();
        }
    }
}
