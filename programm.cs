using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GeneticSearch
{
    class Program
    {
        // Структура для хранения данных о белках
        struct GeneticData
        {
            public string protein;     // название белка
            public string organism;    // название организма
            public string amino_acids; // цепочка аминокислот 
        }

        // Исправленный метод декодирования RLE (поддерживает многозначные числа, например 12A)
        static string RLDecoding(string amino_acids)
        {
            if (string.IsNullOrEmpty(amino_acids)) return string.Empty;

            StringBuilder decoded = new StringBuilder();
            
            for (int i = 0; i < amino_acids.Length; i++)
            {
                // Если встретили цифру — собираем всё число целиком
                if (char.IsDigit(amino_acids[i]))
                {
                    StringBuilder numberBuilder = new StringBuilder();
                    while (i < amino_acids.Length && char.IsDigit(amino_acids[i]))
                    {
                        numberBuilder.Append(amino_acids[i]);
                        i++;
                    }
                    
                    int count = int.Parse(numberBuilder.ToString());
                    char letter = amino_acids[i]; // Текущий символ — это сама аминокислота

                    decoded.Append(letter, count);
                }
                else
                {
                    decoded.Append(amino_acids[i]);
                }
            }
            return decoded.ToString();
        }

        // Метод для чтения базы белков из файла
        static List<GeneticData> ReadData(string filename)
        {
            List<GeneticData> data = new List<GeneticData>();

            if (!File.Exists(filename))
            {
                Console.WriteLine($"Ошибка: Файл данных {filename} не найден!");
                return data;
            }

            using (StreamReader reader = new StreamReader(filename))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // Разделяем строку по табуляции
                    string[] parts = line.Split('\t');
                    if (parts.Length >= 3)
                    {
                        GeneticData protein;
                        protein.protein = parts[0];
                        protein.organism = parts[1];
                        protein.amino_acids = parts[2];
                        data.Add(protein);
                    }
                }
            }
            return data;
        }

        // Главный обработчик команд, выполняющий чтение и запись «на лету»
        static void CommandHandler(List<GeneticData> proteins, string commandsFilename, string outputFilename)
        {
            if (!File.Exists(commandsFilename))
            {
                Console.WriteLine($"Ошибка: Файл команд {commandsFilename} не найден!");
                return;
            }

            using (StreamReader reader = new StreamReader(commandsFilename))
            using (StreamWriter writer = new StreamWriter(outputFilename, false, Encoding.UTF8))
            {
                writer.WriteLine("Ivan Ivanov"); // Замените на ваше имя при сдаче
                writer.WriteLine("Genetic Searching");
                writer.WriteLine("--------------------------------------------------------------------------");

                int commandCounter = 1;

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] parts = line.Split('\t');
                    string cmdName = parts[0];
                    string p1 = parts.Length > 1 ? parts[1] : string.Empty;
                    string p2 = parts.Length > 2 ? parts[2] : string.Empty;

                    string cmdNum = commandCounter.ToString("D3");
                    commandCounter++;

                    if (cmdName == "search")
                    {
                        string decodedTarget = RLDecoding(p1);
                        writer.WriteLine($"{cmdNum}   search   {decodedTarget} ");
                        writer.WriteLine("organism\t\t\tprotein ");

                        bool found = false;
                        foreach (var protein in proteins)
                        {
                            string decodedSequence = RLDecoding(protein.amino_acids);
                            if (decodedSequence.Contains(decodedTarget))
                            {
                                writer.WriteLine($"{protein.organism}\t\t{protein.protein}");
                                found = true;
                            }
                        }

                        if (!found)
                        {
                            writer.WriteLine("NOT FOUND");
                        }
                        writer.WriteLine("--------------------------------------------------------------------------");
                    }
                    else if (cmdName == "diff")
                    {
                        writer.WriteLine($"{cmdNum}   diff   {p1}   {p2} ");
                        writer.WriteLine("amino-acids difference: ");

                        var protein1 = proteins.FirstOrDefault(p => p.protein == p1);
                        var protein2 = proteins.FirstOrDefault(p => p.protein == p2);

                        bool p1Missing = string.IsNullOrEmpty(protein1.protein);
                        bool p2Missing = string.IsNullOrEmpty(protein2.protein);

                        if (p1Missing || p2Missing)
                        {
                            List<string> missingProteins = new List<string>();
                            if (p1Missing) missingProteins.Add(p1);
                            if (p2Missing) missingProteins.Add(p2);

                            writer.WriteLine("MISSING: " + string.Join(", ", missingProteins));
                        }
                        else
                        {
                            string seq1 = RLDecoding(protein1.amino_acids);
                            string seq2 = RLDecoding(protein2.amino_acids);

                            int minLength = Math.Min(seq1.Length, seq2.Length);
                            int maxLength = Math.Max(seq1.Length, seq2.Length);
                            int diffCount = 0;

                            for (int j = 0; j < minLength; j++)
                            {
                                if (seq1[j] != seq2[j]) diffCount++;
                            }
                            diffCount += (maxLength - minLength);

                            writer.WriteLine(diffCount);
                        }
                        writer.WriteLine("--------------------------------------------------------------------------");
                    }
                    else if (cmdName == "mode")
                    {
                        writer.WriteLine($"{cmdNum}   mode   {p1}  ");
                        writer.WriteLine("amino-acid occurs: ");

                        var protein = proteins.FirstOrDefault(p => p.protein == p1);

                        if (string.IsNullOrEmpty(protein.protein))
                        {
                            writer.WriteLine($"MISSING: {p1}");
                        }
                        else
                        {
                            string seq = RLDecoding(protein.amino_acids);
                            
                            if (string.IsNullOrEmpty(seq))
                            {
                                writer.WriteLine("SEQUENCE IS EMPTY");
                            }
                            else
                            {
                                Dictionary<char, int> counts = new Dictionary<char, int>();
                                foreach (char ch in seq)
                                {
                                    if (counts.ContainsKey(ch)) counts[ch]++;
                                    else counts[ch] = 1;
                                }

                                var mostFrequent = counts
                                    .OrderByDescending(kv => kv.Value)
                                    .ThenBy(kv => kv.Key)
                                    .First(); // Безопасно, так как мы проверили, что строка не пустая

                                writer.WriteLine($"{mostFrequent.Key}          {mostFrequent.Value}");
                            }
                        }
                        writer.WriteLine("--------------------------------------------------------------------------");
                    }
                }
            }
            Console.WriteLine($"Обработка завершена! Результат сохранен в {outputFilename}");
        }

        // ТОЧКА ВХОДА (Обрабатывает по очереди 3 набора файлов)
        static void Main(string[] args)
        {
            // Цикл от 1 до 3 для автоматической сборки имен файлов
            for (int i = 1; i <= 3; i++)
            {
                string seqFile = $"sequences{i}.txt";
                string cmdFile = $"commands{i}.txt";
                string outFile = $"genedata{i}.txt";

                Console.WriteLine($"=== Обработка набора файлов №{i} ===");

                // Проверяем, на месте ли входные файлы
                if (File.Exists(seqFile) && File.Exists(cmdFile))
                {
                    // Читаем базу белков текущего набора (например, sequences1.txt)
                    List<GeneticData> proteins = ReadData(seqFile);

// Выполняем команды из commands1.txt и записываем результат в genedata1.txt
CommandHandler(proteins, cmdFile, outFile);
}
else
{
    if (!File.Exists(seqFile)) Console.WriteLine($"Предупреждение: Файл {seqFile} не найден.");
    if (!File.Exists(cmdFile)) Console.WriteLine($"Предупреждение: Файл {cmdFile} не найден.");
    }
    Console.WriteLine("---------------------------------------------\n");
    }
    Console.WriteLine("Все наборы файлов обработаны!");
    Console.WriteLine("Нажмите любую клавишу для выхода...");
    Console.ReadKey();
        }
        }
        }
