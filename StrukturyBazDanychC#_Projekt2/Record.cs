using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrukturyBazDanychC__Projekt2
{
    public enum FlagType
    {
        none,
        deleted,
        empty
    }
    public class Record
    {
        public int Key { get; set; }
        public string Value { get; set; }
        public int OverFlowPointer_Key { get; set; }

        public Record(string value, int key)
        {
            Value = value;
            Key = key;
            OverFlowPointer_Key = -1;

            // Dopasowanie długości stringa do 30 znaków
            if (value.Length > 30)
            {
                Value = value.Substring(0, 30); 
            }
            else
            {
                Value = value.PadRight(30); 
            }
        }

        public static Record EmptyRecord()
        {
            return new Record(new string('*', 30), -1)
            {
                OverFlowPointer_Key = -1
            };
        }

    }
}
