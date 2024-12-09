using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrukturyBazDanychC__Projekt2
{
    public class PagedFile
    {
        public int AmountOfPages { get; set; }
        public int AmountOfAllRecords { get; set; }
        public int AmountOfOverflowRecords;
        public int MaxAmountOfRecordsOnPage { get; set; }
        public const int RecordSize = 38;
        public string MainFileName { get; set; }
        private double Beta { get; set; }
        private double Alpha { get; set; }
        public PagedFile(int MaxRecords, DiskInformations DiskInformation, double beta, double alpha)
        {
            AmountOfPages = 0;
            AmountOfAllRecords = 0;
            AmountOfOverflowRecords = 0;
            MaxAmountOfRecordsOnPage = MaxRecords;
            Beta = beta;
            Alpha = alpha;
            MainFileName = "main.dat";
            using (var fs = new FileStream("index.idx", FileMode.Create, FileAccess.Write)) ;
            using (var fs = new FileStream("main.dat", FileMode.Create, FileAccess.Write)) ;
            using (var fs = new FileStream("overflow.dat", FileMode.Create, FileAccess.Write)) ;
            InitializeFirstRecord(DiskInformation);

        }

        public void AddRecord(Record NewRecord, DiskInformations DiskInformation)
        {
            int PageNumber = FindPage(NewRecord.Key, DiskInformation);
            Page _Page = new Page(MaxAmountOfRecordsOnPage, PageNumber);

            _Page.ReadPageFromFile(MainFileName);
            DiskInformation.ReadFromDisk++;
            if (_Page.AddRecordToPage(NewRecord, ref AmountOfOverflowRecords, DiskInformation)) AmountOfAllRecords++;
            _Page.WritePageToFile(MainFileName);
            DiskInformation.WriteToDisk++;


            double ratio = (double)AmountOfOverflowRecords / AmountOfAllRecords;
            if (ratio > Beta)
            {
                Console.WriteLine("Wykonano automatyczna reorganizacje!");
                Reorganization(MaxAmountOfRecordsOnPage, Alpha, DiskInformation);
            }

        }

        public void DeleteRecord(int Key, DiskInformations DiskInformation)
        {
            int PageNumber = FindPage(Key, DiskInformation);

            Page _Page = new Page(MaxAmountOfRecordsOnPage, PageNumber);
            _Page.ReadPageFromFile(MainFileName);
            DiskInformation.ReadFromDisk++;
            if (_Page.DeleteRecordFromPage(Key, ref AmountOfOverflowRecords, AmountOfPages, DiskInformation)) AmountOfAllRecords--;
            _Page.WritePageToFile(MainFileName);
            DiskInformation.WriteToDisk++;
        }

        public void ShowAllPages(DiskInformations DiskInformation)
        {
            Console.WriteLine("Main File Pages: ");
            for (int i = 0; i < AmountOfPages; i++)
            {
                Page PageTmp = new Page(MaxAmountOfRecordsOnPage, i);
                PageTmp.ReadPageFromFile(MainFileName);
                DiskInformation.ReadFromDisk++;
                Console.WriteLine("Page " + i + ": ");
                PageTmp.ShowPage();
            }

            Console.WriteLine();
            OverFlowFile OverFlowFileTmp = new OverFlowFile(AmountOfOverflowRecords);
            OverFlowFileTmp.ReadOverFlowRecords("overflow.dat", AmountOfOverflowRecords);
            DiskInformation.ReadFromDisk++;
            OverFlowFileTmp.ShowOverFlowFile();
            Console.WriteLine();
        }

        public void InitializeFirstRecord(DiskInformations DiskInformation)
        {

            IndexFile _IndexFile = new IndexFile(AmountOfPages);
            _IndexFile.ReadIndexes(AmountOfPages);
            DiskInformation.ReadFromDisk++;
            AmountOfPages++;
            _IndexFile.Indexes.Add(-2999);
            _IndexFile.WriteIndexes();
            DiskInformation.WriteToDisk++;
            AddRecord(new Record("Guard", -2999), DiskInformation);
        }

        public int FindPage(int Key, DiskInformations DiskInformation)
        {

            IndexFile _IndexFile = new IndexFile(AmountOfPages);
            _IndexFile.ReadIndexes(AmountOfPages);
            DiskInformation.ReadFromDisk++;

            if (AmountOfPages == 1) return 0;
            for (int i = 0; i < AmountOfPages - 1; i++)
            {
                if (_IndexFile.Indexes[i] <= Key && _IndexFile.Indexes[i + 1] > Key)
                {
                    return i;
                }
            }
            return _IndexFile.Indexes.Count - 1;
        }

        public string GiveValueOfRecord(int Key, DiskInformations DiskInformation)
        {
            // szukamy strony na której może znajdować się rekord
            Page _PageTmp = new Page(MaxAmountOfRecordsOnPage, FindPage(Key, DiskInformation));
            _PageTmp.ReadPageFromFile(MainFileName);
            DiskInformation.ReadFromDisk++;
            for (int j = 0; j < _PageTmp.AmountOfRecords; j++)
            {
                if (_PageTmp.Records[j].Key > Key)
                {
                    Console.WriteLine("Nie ma takiego rekordu o kluczu: " + Key);
                    return null;
                }
                else
                {
                    if (_PageTmp.Records[j].Key == Key)
                    {
                        Console.WriteLine("Rekord o kluczu " + Key + " ma wartość: " + _PageTmp.Records[j].Value);
                        return _PageTmp.Records[j].Value;
                    }
                    if (_PageTmp.Records[j].OverFlowPointer_Key != -1)
                    {
                        OverFlowFile OverFlowFileTmp = new OverFlowFile(AmountOfOverflowRecords);
                        OverFlowFileTmp.ReadOverFlowRecords("overflow.dat", AmountOfOverflowRecords);
                        DiskInformation.ReadFromDisk++;
                        //szukanie pierwszego nastepcy w overflow
                        for (int i = 0; i < AmountOfOverflowRecords; i++)
                        {
                            if (OverFlowFileTmp.OverFlowRecords[i].Key == Key)
                            {
                                Console.WriteLine("Rekord o kluczu " + Key + " ma wartość: " + OverFlowFileTmp.OverFlowRecords[i].Value);
                                return OverFlowFileTmp.OverFlowRecords[i].Value;
                            }
                        }
                    }
                }
            }
            Console.WriteLine("Nie ma takiego rekordu o kluczu: " + Key);
            return null;
        }


        public void Reorganization(int maxAmountOfRecords, double alpha, DiskInformations DiskInformation)
        {
            //Utworzenie nowej struktury dla indeksow
            IndexFile New_IndexFile = new IndexFile(0);
            int New_AmountOfPages = 0;
            Page New_Page = new Page(maxAmountOfRecords, New_AmountOfPages);
            int AmountOfRecordsInNewPage = 0;

            for (int i = 0; i < AmountOfPages; i++)
            {
                Page PageOriginal = new Page(maxAmountOfRecords, i);
                PageOriginal.ReadPageFromFile(MainFileName);
                DiskInformation.ReadFromDisk++;

                for (int j = 0; j < PageOriginal.AmountOfRecords; j++)
                {
                    //Dodajemy nowy indeks, gdy zaczynamy nową strone
                    if (AmountOfRecordsInNewPage == 0)
                    {
                        New_IndexFile.Indexes.Add(PageOriginal.Records[j].Key);
                    }
                    New_Page.Records[AmountOfRecordsInNewPage] = (new Record(PageOriginal.Records[j].Value, PageOriginal.Records[j].Key)
                    {
                        OverFlowPointer_Key = -1,
                    });
                    AmountOfRecordsInNewPage++;

                    //Gdy liczba rekordów na stronie przekroczy pewien próg, to zapisujemy strone oraz tworzymy nową
                    if (AmountOfRecordsInNewPage >= maxAmountOfRecords * alpha)
                    {
                        for (int l = AmountOfRecordsInNewPage; l < maxAmountOfRecords; l++)
                        {
                            New_Page.Records[l] = Record.EmptyRecord();
                        }
                        New_Page.WritePageToFile(GiveOppositeMainFileName());
                        DiskInformation.WriteToDisk++;
                        New_AmountOfPages++;
                        New_Page = new Page(maxAmountOfRecords, New_AmountOfPages);
                        AmountOfRecordsInNewPage = 0;
                    }
                    //Obecny rekord z mainArea ma wskaźnik na overflow 
                    if (PageOriginal.Records[j].OverFlowPointer_Key != -1)
                    {
                        int NextOverFlowPointer = PageOriginal.Records[j].OverFlowPointer_Key;
                        OverFlowFile OverFlowFileTmp = new OverFlowFile(AmountOfOverflowRecords);
                        OverFlowFileTmp.ReadOverFlowRecords("overflow.dat", AmountOfOverflowRecords);
                        DiskInformation.ReadFromDisk++;

                        int FindedIndexInOverFlow = -1;

                        //szukanie pierwszego nastepcy w overflow
                        for (int k = 0; k < AmountOfOverflowRecords; k++)
                        {
                            if (OverFlowFileTmp.OverFlowRecords[k].Key == NextOverFlowPointer)
                            {
                                FindedIndexInOverFlow = k;
                                break;
                            }
                        }
                        // Zczytywanie wszystkich rekordów z ciągu wskaźników 
                        while (true)
                        {
                            int key = OverFlowFileTmp.OverFlowRecords[FindedIndexInOverFlow].Key;
                            string value = OverFlowFileTmp.OverFlowRecords[FindedIndexInOverFlow].Value;

                            //Dodawanie nowego indeksu
                            if (AmountOfRecordsInNewPage == 0)
                            {
                                New_IndexFile.Indexes.Add(key);
                            }
                            New_Page.Records[AmountOfRecordsInNewPage] = (new Record(value, key)
                            {
                                OverFlowPointer_Key = -1,
                            });
                            AmountOfRecordsInNewPage++;

                            //Gdy liczba rekordów na stronie przekroczy pewien próg, to zapisujemy strone oraz tworzymy nową
                            if (AmountOfRecordsInNewPage >= maxAmountOfRecords * alpha)
                            {
                                for (int l = AmountOfRecordsInNewPage; l < maxAmountOfRecords; l++)
                                {
                                    New_Page.Records[l] = Record.EmptyRecord();
                                }
                                New_Page.WritePageToFile(GiveOppositeMainFileName());
                                DiskInformation.WriteToDisk++;
                                New_AmountOfPages++;
                                New_Page = new Page(maxAmountOfRecords, New_AmountOfPages);
                                AmountOfRecordsInNewPage = 0;
                            }
                            if (OverFlowFileTmp.OverFlowRecords[FindedIndexInOverFlow].OverFlowPointer_Key == -1) break;
                            FindedIndexInOverFlow++;
                        }
                    }
                }
            }
            //dodanie ostatniej strony gdzie nie ma rekordow wiecej niz maxAmountOfRecords * alpha
            if (AmountOfRecordsInNewPage > 0)
            {
                for (int l = AmountOfRecordsInNewPage; l < maxAmountOfRecords; l++)
                {
                    New_Page.Records[l] = Record.EmptyRecord();
                }
                New_Page.WritePageToFile(GiveOppositeMainFileName());
                DiskInformation.WriteToDisk++;
                New_AmountOfPages++;
            }

            using (var fs = new FileStream("index.idx", FileMode.Create, FileAccess.Write)) ;
            New_IndexFile.WriteIndexes();
            DiskInformation.WriteToDisk++;
            ChangeMainFile();
            using (var fs = new FileStream(GiveOppositeMainFileName(), FileMode.Create, FileAccess.Write)) ;
            using (var fs = new FileStream("overflow.dat", FileMode.Create, FileAccess.Write)) ;
            AmountOfOverflowRecords = 0;
            AmountOfPages = New_AmountOfPages;
        }

        public void ShowAllRecords(DiskInformations DiskInformation)
        {
            OverFlowFile OverFlowFileTmp = new OverFlowFile(AmountOfOverflowRecords);
            OverFlowFileTmp.ReadOverFlowRecords("overflow.dat", AmountOfOverflowRecords);
            DiskInformation.ReadFromDisk++;
            Console.WriteLine("All records:");
            for (int i = 0; i < AmountOfPages; i++)
            {
                Page PageTmp = new Page(MaxAmountOfRecordsOnPage, i);
                PageTmp.ReadPageFromFile(MainFileName);
                DiskInformation.ReadFromDisk++;
                for (int k = 0; k < PageTmp.AmountOfRecords; k++)
                {
                    Console.WriteLine("Key: " + PageTmp.Records[k].Key + " Value: " + PageTmp.Records[k].Value);
                    if (PageTmp.Records[k].OverFlowPointer_Key != -1)
                    {

                        int FindedIndexInOverFlow = -1;

                        //szukanie pierwszego nastepcy w overflow
                        for (int j = 0; j < AmountOfOverflowRecords; j++)
                        {
                            if (OverFlowFileTmp.OverFlowRecords[j].Key == PageTmp.Records[k].OverFlowPointer_Key)
                            {
                                FindedIndexInOverFlow = j;
                                break;
                            }
                        }

                        while (true)
                        {
                            Console.WriteLine("Key: " + OverFlowFileTmp.OverFlowRecords[FindedIndexInOverFlow].Key
                                + " Value: " + OverFlowFileTmp.OverFlowRecords[FindedIndexInOverFlow].Value);

                            if (OverFlowFileTmp.OverFlowRecords[FindedIndexInOverFlow].OverFlowPointer_Key == -1) break;
                            FindedIndexInOverFlow++;
                        }
                    }
                }
            }
        }

        public void ChangeMainFile()
        {
            if (MainFileName == "main.dat")
            {
                MainFileName = "main2.dat";
            }
            else
            {
                MainFileName = "main.dat";
            }
        }
        public string GiveOppositeMainFileName()
        {
            if (MainFileName == "main.dat")
            {
                return "main2.dat";
            }
            else
            {
                return "main.dat";
            }
        }
    }
}
