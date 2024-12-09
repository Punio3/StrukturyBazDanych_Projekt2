
using StrukturyBazDanychC__Projekt2;
using System.Formats.Tar;
using System.Text;

class Program
{

    static void GenerateTestOperations(string filePath, int numberOfOperations)
    {
        Random random = new Random();
        StringBuilder operations = new StringBuilder();

        for (int i = 0; i < numberOfOperations; i++)
        {
            int operationType = random.Next(1, 5); // Operacje: 1-4 (odczyt, dodanie, usunięcie, aktualizacja)
            int key = random.Next(1, 10000);
            string value = RandomString(30, random);

            switch (operationType)
            {
                case 1: // Odczytaj rekord
                    operations.AppendLine($"1 {key}");
                    break;
                case 2: // Wstaw rekord
                    operations.AppendLine($"2 {key} {value}");
                    break;
                case 3: // Usuń rekord
                    operations.AppendLine($"3 {key}");
                    break;
                case 4: // Aktualizuj rekord
                    int newKey = random.Next(1, 10000);
                    string newValue = RandomString(30, random);
                    operations.AppendLine($"4 {key} {newKey} {newValue}");
                    break;
            }
        }

        File.WriteAllText(filePath, operations.ToString());
        Console.WriteLine($"Wygenerowano {numberOfOperations} operacji testowych i zapisano do pliku: {filePath}");
    }

    static string RandomString(int length, Random random)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        char[] stringChars = new char[length];

        for (int i = 0; i < length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        return new string(stringChars);
    }

    static void ExecuteOperationsFromFile(string filePath, PagedFile pagedFile, DiskInformations diskInformations)
    {
        string[] lines = File.ReadAllLines(filePath);

        foreach (string line in lines)
        {
            string[] parts = line.Split(' ');
            int operationType = int.Parse(parts[0]);

            switch (operationType)
            {
                case 1: // Odczytaj rekord
                    int readKey = int.Parse(parts[1]);
                    pagedFile.GiveValueOfRecord(readKey, diskInformations);
                    break;
                case 2: // Wstaw rekord
                    int insertKey = int.Parse(parts[1]);
                    string insertValue = parts[2];
                    Record newRecord = new Record(insertValue, insertKey);
                    pagedFile.AddRecord(newRecord, diskInformations);
                    break;
                case 3: // Usuń rekord
                    int deleteKey = int.Parse(parts[1]);
                    pagedFile.DeleteRecord(deleteKey, diskInformations);
                    break;
                case 4: // Aktualizuj rekord
                    int updateOldKey = int.Parse(parts[1]);
                    int updateNewKey = int.Parse(parts[2]);
                    string updateNewValue = parts[3];
                    pagedFile.DeleteRecord(updateOldKey, diskInformations);
                    pagedFile.AddRecord(new Record(updateNewValue, updateNewKey), diskInformations);
                    break;
                default:
                    Console.WriteLine($"Nieznana operacja: {operationType}");
                    break;
            }

        }

        Console.WriteLine("Wszystkie operacje z pliku zostały wykonane.");
    }

    static void ShowMenu()
    {
        Console.WriteLine("Author: Przemek Dębek       Numer albumu: 193378");
        Console.WriteLine("Wybierz:\r\n1) odczytaj rekord\r\n2) wstaw rekord\r\n3) usuń rekord\r\n4) " +
            "aktualizuj rekord\r\n5) reorganizuj plik\r\n6) przejrzyj zawartość pliku\r\n7) przejrzyj zawartość indeksu\r\n8) " +
            "wczytaj plik testowy\r\n9) odczytaj wszystkie rekordy\r\n10)Generuj dodanie rekordow\r\n0) wyjscie");
    }

    static void WstawRekord(PagedFile PagedFile, DiskInformations DiskInformation)
    {
        int Key;
        string Value;

        Console.Write("Klucz: ");
        int.TryParse(Console.ReadLine(), out Key);
        Console.Write("Wartosc: ");
        Value = Console.ReadLine();

        Record NewRecord = new Record(Value, Key);


        PagedFile.AddRecord(NewRecord, DiskInformation);
    }
    static void ZnajdzRekord(PagedFile _PageFile, DiskInformations DiskInformation)
    {
        int Key;

        Console.Write("Podaj klucz: ");
        int.TryParse(Console.ReadLine(), out Key);

        _PageFile.GiveValueOfRecord(Key, DiskInformation);
    }

    static void UpdateRekord(PagedFile _PageFile, DiskInformations DiskInformation)
    {
        int Key;
        int NewKey;
        string Value;

        Console.Write("Podaj klucz rekordu do usunięcia: ");
        int.TryParse(Console.ReadLine(), out Key);
        Console.Write("Podaj nowy klucz: ");
        int.TryParse(Console.ReadLine(), out NewKey);
        Console.Write("Podaj nową wartosc: ");
        Value = Console.ReadLine();

        if (_PageFile.GiveValueOfRecord(Key, DiskInformation) == null)
        {
            Console.WriteLine("Klucz o wartosci: " + Key + " nie istnieje. Nie wykonano aktualizacji.");
            return;
        }
        if (_PageFile.GiveValueOfRecord(NewKey, DiskInformation) != null)
        {
            Console.WriteLine("Klucz o wartosci: " + NewKey + " juz istnieje. Nie wykonano aktualizacji.");
            return;
        }
        _PageFile.DeleteRecord(Key, DiskInformation);
        _PageFile.AddRecord(new Record(Value, NewKey), DiskInformation);

    }

    static void UsunRekord(PagedFile _PageFile, DiskInformations DiskInformation)
    {
        int Key;

        Console.Write("Podaj klucz: ");
        int.TryParse(Console.ReadLine(), out Key);        

        _PageFile.DeleteRecord(Key, DiskInformation);
    }
    static void ShowAllIndexes(PagedFile _PageFile, DiskInformations DiskInformation)
    {
        IndexFile IndexFileTmp = new IndexFile(_PageFile.AmountOfPages);
        IndexFileTmp.ReadIndexes(_PageFile.AmountOfPages);
        IndexFileTmp.ShowIndexes();
        DiskInformation.ReadFromDisk++;
    }
    static void Main(string[] args)
    {
        int mode = 0;
        int option = -1;
        int MaxRecords = 10;
        double alpha = 1;
        double beta = 0.9;
        DiskInformations _DiskInformations = new DiskInformations();

        PagedFile _PagedFile = new PagedFile(MaxRecords, _DiskInformations, beta, alpha);

        Console.WriteLine("Wybierz tryb działania programu: \n 0) Zczytanie z pliku\n 1) Wczytywanie z klawiatury");
        int.TryParse(Console.ReadLine(), out mode);
        if (mode == 0)
        {
            int AmountOfOperations;
            Console.WriteLine("Podaj liczbe operacji lub wpisz -1 aby nie zmieniac pliku");
            int.TryParse(Console.ReadLine(), out AmountOfOperations);
            if (AmountOfOperations != -1)
            {
                GenerateTestOperations("test.dat", AmountOfOperations);
                ExecuteOperationsFromFile("test.dat", _PagedFile, _DiskInformations);
            }
            else
            {
                ExecuteOperationsFromFile("test.dat", _PagedFile, _DiskInformations);
            }
            _DiskInformations.ShowDiskParameters();
        }
        else
        {
            while (true)
            {
                ShowMenu();
                int.TryParse(Console.ReadLine(), out option);
                switch (option)
                {
                    case 0:
                        return;
                    case 1:
                        ZnajdzRekord(_PagedFile, _DiskInformations);
                        break;
                    case 2:
                        WstawRekord(_PagedFile, _DiskInformations);
                        break;
                    case 3:
                        UsunRekord(_PagedFile, _DiskInformations);
                        break;
                    case 4:
                        UpdateRekord(_PagedFile, _DiskInformations);
                        break;
                    case 5:
                        _PagedFile.Reorganization(MaxRecords, alpha, _DiskInformations);
                        break;
                    case 6:
                        _PagedFile.ShowAllPages(_DiskInformations);
                        break;
                    case 7:
                        ShowAllIndexes(_PagedFile, _DiskInformations);
                        break;
                    case 9:
                        _PagedFile.ShowAllRecords(_DiskInformations);
                        break;
                    default:
                        break;
                }
                _DiskInformations.ShowDiskParameters();
                _DiskInformations.ResetParameters();
            }

        }
    }
}
