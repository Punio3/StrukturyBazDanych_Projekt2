using System;
using System.IO;

namespace StrukturyBazDanychC__Projekt2
{
    public class Page
    {
        public Record[] Records; // Rekordy na stronie
        public int AmountOfRecords { get; set; } // Ilość rzeczywistych rekordów
        public int PageNumber { get; set; } // Numer strony

        public const int RecordSize = 38; // Stały rozmiar rekordu w bajtach
        public int MaxAmountOfRecords { get; } // Maksymalna liczba rekordów na stronie

        public Page(int maxAmountOfRecords, int pageNumber)
        {
            Records = new Record[maxAmountOfRecords];
            AmountOfRecords = 0;
            PageNumber = pageNumber;
            MaxAmountOfRecords = maxAmountOfRecords;
        }

        public void ReadPageFromFile(string filePath)
        {
            //obliczanie przesuniecia w pliku
            long pageOffset = PageNumber * MaxAmountOfRecords * RecordSize;
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fs))
            {
                fs.Seek(pageOffset, SeekOrigin.Begin);

                for (int i = 0; i < MaxAmountOfRecords; i++)
                {
                    try
                    {
                        // Odczyt danych rekordu
                        int key = reader.ReadInt32();
                        string value = new string(reader.ReadChars(30)).TrimEnd();
                        int overflowPointer = reader.ReadInt32();

                        // Tworzenie obiektu Record i dodanie do strony
                        Records[i] = new Record(value, key)
                        {
                            OverFlowPointer_Key = overflowPointer,

                        };
                        if (Records[i].Key != -1) AmountOfRecords++;
                    }
                    catch (EndOfStreamException)
                    {
                        Records[i] = Record.EmptyRecord();

                    }
                }
            }
        }

        public void WritePageToFile(string filePath)
        {
            //obliczanie przesuniecia w pliku
            long pageOffset = PageNumber * MaxAmountOfRecords * RecordSize;
            using (var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
            using (var writer = new BinaryWriter(fs))
            {
                fs.Seek(pageOffset, SeekOrigin.Begin);

                foreach (var record in Records)
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




        public bool AddRecordToPage(Record record, ref int AmountOfOverflowRecords, DiskInformations DiskInformation)
        {
            //Mozna dodac rekord w mainarea ( nie jest plena strona )
            if (AmountOfRecords < MaxAmountOfRecords)
            {
                SortRecordsInPage(record);

            }
            else
            {
                //szukanie rekordu od którego będziemy zaczynać poszukiwanie miejsca w overflowArea
                int insertIndex = 0;
                for (int i = 0; i < AmountOfRecords; i++)
                {
                    if (Records[i].Key == record.Key)
                    {
                        Console.WriteLine("Rekord o takim kluczu już istnieje!");
                        return false;
                    }
                    if (Records[i].Key > record.Key)
                    {
                        insertIndex = i - 1;
                        break;
                    }
                    insertIndex = AmountOfRecords - 1;
                }


                OverFlowFile OverFlowFileTmp = new OverFlowFile(AmountOfOverflowRecords);
                OverFlowFileTmp.ReadOverFlowRecords("overflow.dat", AmountOfOverflowRecords);
                DiskInformation.ReadFromDisk++;
                //Odnaleziony rekord w mainarea nie ma jeszcze wskaznika
                if (Records[insertIndex].OverFlowPointer_Key == -1)
                {
                    Records[insertIndex].OverFlowPointer_Key = record.Key;

                }
                else
                {
                    int FindedIndexInOverFlow = -1;

                    //Przeszukiwanie OverFlowArea, aby odnaleźć pierwszy rekord od którego zaczynamy przeskakiwanie po wskaźnikach
                    for (int i = 0; i < AmountOfOverflowRecords; i++)
                    {
                        if (OverFlowFileTmp.OverFlowRecords[i].Key == Records[insertIndex].OverFlowPointer_Key)
                        {
                            FindedIndexInOverFlow = i;
                            break;
                        }
                    }
                    //case gdy pierwszy rekord w overflowarea jest wiekszy od rekordu, który chcemy dodać i trzeba zaktualizować wskaźnik w mainarea
                    if (OverFlowFileTmp.OverFlowRecords[FindedIndexInOverFlow].Key > record.Key)
                    {
                        Records[insertIndex].OverFlowPointer_Key = record.Key;
                        record.OverFlowPointer_Key = OverFlowFileTmp.OverFlowRecords[FindedIndexInOverFlow].Key;
                    }
                    else
                    {
                        //Przeskakiwanie po wskaźnikach, aż znajdziemy miejsce dla nowego rekordu
                        while (true)
                        {
                            //klucz juz istnieje
                            if (OverFlowFileTmp.OverFlowRecords[FindedIndexInOverFlow].Key == record.Key)
                            {
                                Console.WriteLine("Rekord o takim kluczu już istnieje!");
                                return false;
                            }
                            //trzeba dodac rekord pomiedzy 2 rekordy
                            if (OverFlowFileTmp.OverFlowRecords[FindedIndexInOverFlow].Key > record.Key)
                            {
                                OverFlowFileTmp.OverFlowRecords[FindedIndexInOverFlow - 1].OverFlowPointer_Key = record.Key;
                                record.OverFlowPointer_Key = OverFlowFileTmp.OverFlowRecords[FindedIndexInOverFlow].Key;
                                break;
                            }
                            //trzeba dodac rekord na sam koniec
                            if (OverFlowFileTmp.OverFlowRecords[FindedIndexInOverFlow].OverFlowPointer_Key == -1)
                            {
                                OverFlowFileTmp.OverFlowRecords[FindedIndexInOverFlow].OverFlowPointer_Key = record.Key;
                                break;
                            }

                            FindedIndexInOverFlow++;
                        }
                    }
                }
                AmountOfOverflowRecords++;
                OverFlowFileTmp.OverFlowRecords.Add(record);
                SortRecordsInOverFlowArea(record, AmountOfOverflowRecords, OverFlowFileTmp.OverFlowRecords);
                OverFlowFileTmp.WriteOverFlowToFile("overflow.dat");
                DiskInformation.WriteToDisk++;
            }
            return true;
        }


        public bool DeleteRecordFromPage(int Key, ref int AmountOfOverflowRecords, int AmountOfPages, DiskInformations DiskInformation)
        {
            for (int k = 0; k < AmountOfRecords; k++)
            {
                //rekord do usuniecia jest w MainArea
                if (Records[k].Key == Key)
                {
                    //Sprawdzamy czy wskaźnik jest pusty
                    if (Records[k].OverFlowPointer_Key == -1)
                    {
                        Records[k].Key = Records[AmountOfRecords - 1].Key + 10;
                        SortRecordsInPage();
                        Records[AmountOfRecords - 1] = Record.EmptyRecord();
                    }
                    else
                    {
                        OverFlowFile OverFlowFileTmp = new OverFlowFile(AmountOfOverflowRecords);
                        OverFlowFileTmp.ReadOverFlowRecords("overflow.dat", AmountOfOverflowRecords);
                        DiskInformation.ReadFromDisk++;

                        int FindedIndexInOverFlow = -1;

                        //szukanie pierwszego nastepcy w overflow
                        for (int i = 0; i < AmountOfOverflowRecords; i++)
                        {
                            if (OverFlowFileTmp.OverFlowRecords[i].Key == Records[k].OverFlowPointer_Key)
                            {
                                FindedIndexInOverFlow = i;
                                break;
                            }
                        }
                        Records[k] = OverFlowFileTmp.OverFlowRecords[FindedIndexInOverFlow];
                        MoveRecordsInOverFlowArea(AmountOfOverflowRecords, OverFlowFileTmp.OverFlowRecords, FindedIndexInOverFlow);
                        AmountOfOverflowRecords--;
                        OverFlowFileTmp.WriteOverFlowToFile("overflow.dat");
                        DiskInformation.WriteToDisk++;
                    }
                    //jezeli pierwszy rekord ze strony zostaje usuniety to musimy w index file zaaktualizowac wartosc, gdyz moga wtedy wyjsc bledy jak tego nie zrobimy
                    if (k == 0)
                    {
                        if (Records[0].Key != -1)
                        {
                            IndexFile _IndexFile = new IndexFile(AmountOfPages);
                            _IndexFile.ReadIndexes(AmountOfPages);
                            DiskInformation.ReadFromDisk++;

                            _IndexFile.Indexes[PageNumber] = Records[0].Key;

                            _IndexFile.WriteIndexes();
                            DiskInformation.WriteToDisk++;
                        }
                    }
                    return true;
                }
                // drugi warunek w ifie jest po to zeby wejsc w ciag wskaznikow najwiekszego
                // rekordu w stronie np. 1,5,10 i 10 wksazuje na 11,12,13 to bez 2 warunku nie wejdziemy w warunek i bedzie pominiety case
                if (Records[k].Key > Key || k == AmountOfRecords - 1)
                {
                    OverFlowFile OverFlowFileTmp = new OverFlowFile(AmountOfOverflowRecords);
                    OverFlowFileTmp.ReadOverFlowRecords("overflow.dat", AmountOfOverflowRecords);
                    DiskInformation.ReadFromDisk++;

                    int FindedIndexInOverFlow = -1;
                    int IndexOfRecordInMainArea = -1;
                    if (k == AmountOfRecords - 1) IndexOfRecordInMainArea = k;
                    else IndexOfRecordInMainArea = k - 1;

                    //szukanie pierwszego nastepcy w overflow
                    for (int i = 0; i < AmountOfOverflowRecords; i++)
                    {
                        if (OverFlowFileTmp.OverFlowRecords[i].Key == Records[IndexOfRecordInMainArea].OverFlowPointer_Key)
                        {
                            FindedIndexInOverFlow = i;
                            break;
                        }
                    }
                    if (FindedIndexInOverFlow != -1)
                    {
                        //usuwamy pierwszy wskaznik i trzeba zaaktualizowac wskaznik w mainarea 
                        if (OverFlowFileTmp.OverFlowRecords[FindedIndexInOverFlow].Key == Key)
                        {
                            Records[IndexOfRecordInMainArea].OverFlowPointer_Key = OverFlowFileTmp.OverFlowRecords[FindedIndexInOverFlow].OverFlowPointer_Key;
                            MoveRecordsInOverFlowArea(AmountOfOverflowRecords, OverFlowFileTmp.OverFlowRecords, FindedIndexInOverFlow);
                            AmountOfOverflowRecords--;
                            OverFlowFileTmp.WriteOverFlowToFile("overflow.dat");
                            DiskInformation.WriteToDisk++;
                            return true;
                        }

                        //usuwamy rekord ktory jest w overflow i nie jest 1 wskaznikiem z mainarea (trzeba aktualizowac wskazniki w overflow dla poprzednika)
                        while (true)
                        {
                            if (OverFlowFileTmp.OverFlowRecords[FindedIndexInOverFlow].Key == Key)
                            {
                                OverFlowFileTmp.OverFlowRecords[FindedIndexInOverFlow - 1].OverFlowPointer_Key = OverFlowFileTmp.OverFlowRecords[FindedIndexInOverFlow].OverFlowPointer_Key;
                                MoveRecordsInOverFlowArea(AmountOfOverflowRecords, OverFlowFileTmp.OverFlowRecords, FindedIndexInOverFlow);
                                AmountOfOverflowRecords--;
                                OverFlowFileTmp.WriteOverFlowToFile("overflow.dat");
                                DiskInformation.WriteToDisk++;
                                return true;
                            }
                            if (OverFlowFileTmp.OverFlowRecords[FindedIndexInOverFlow].OverFlowPointer_Key == -1) break;
                            FindedIndexInOverFlow++;
                        }
                    }
                }
            }

            Console.WriteLine("Rekord o takim kluczu nie istnieje!");
            return false;
        }

        public void SortRecordsInPage(Record record)
        {

            // Przeszukiwanie miejsca dla rekordu tymczasowego
            int insertIndex = 0;
            for (int i = 0; i < AmountOfRecords; i++)
            {
                if (Records[i].Key == record.Key)
                {
                    Console.WriteLine("Rekord o takim kluczu już istnieje!");
                    return;
                }
                if (Records[i].Key > record.Key)
                {
                    insertIndex = i;
                    break;
                }
                insertIndex = AmountOfRecords;
            }

            // Przesuwanie rekordów w prawo od miejsca wstawienia
            for (int j = AmountOfRecords; j > insertIndex; j--)
            {
                Records[j] = Records[j - 1];
            }

            // Wstawienie rekordu tymczasowego w odpowiednie miejsce
            Records[insertIndex] = record;
        }
        public void SortRecordsInOverFlowArea(Record record, int AmountOfOverFlowRecords, List<Record> OverFlowRecords)
        {

            // Przeszukiwanie miejsca dla rekordu tymczasowego
            int insertIndex = 0;
            for (int i = 0; i < AmountOfOverFlowRecords; i++)
            {

                if (OverFlowRecords[i].Key > record.Key)
                {
                    insertIndex = i;
                    break;
                }
                insertIndex = AmountOfOverFlowRecords - 1;
            }

            // Przesuwanie rekordów w prawo od miejsca wstawienia
            for (int j = AmountOfOverFlowRecords - 1; j > insertIndex; j--)
            {
                OverFlowRecords[j] = OverFlowRecords[j - 1];
            }

            // Wstawienie rekordu tymczasowego w odpowiednie miejsce
            OverFlowRecords[insertIndex] = record;
        }
        public void MoveRecordsInOverFlowArea(int AmountOfOverFlowRecords, List<Record> OverFlowRecords, int index)
        {
            for (int j = index; j < AmountOfOverFlowRecords - 1; j++)
            {
                OverFlowRecords[j] = OverFlowRecords[j + 1];
            }

        }
        public void SortRecordsInPage()
        {
            for (int i = 1; i < AmountOfRecords; i++)
            {
                Record current = Records[i];
                int j = i - 1;

                // Przesuwanie większych elementów w prawo
                while (j >= 0 && Records[j].Key > current.Key)
                {
                    Records[j + 1] = Records[j];
                    j--;
                }

                // Wstawienie bieżącego elementu w odpowiednią pozycję
                Records[j + 1] = current;
            }
        }
        public void ShowPage()
        {
            for (int k = 0; k < Records.Length; k++)
            {
                Console.WriteLine("Key: " + Records[k].Key + " Value: " + Records[k].Value
                    + " OverFlowPointer: " + Records[k].OverFlowPointer_Key);
            }
        }
    }
}
