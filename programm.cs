using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GeneticSearch
{
    class Program
    {
        struct Protein
        {
            public string name;        // название белка
            public string organism;    // название организма
            public string amino_acids; // раскодированная цепочка аминокислот
        }

        struct Command
        {
            public string name;
            public string parameter1;
            public string parameter2;
        }

        // Метод декодирования RLE (например, 3Q -> QQQ)
        static string Decoding(string amino_acids)
        {
            if (string.IsNullOrEmpty(amino_acids)) return string.Empty;

            string decoded = string.Empty;
            for (int i = 0; i < amino_acids.Length; i++)
            {
                char ch = amino_acids[i];
                if (char.IsDigit(ch))
                {
                    int count = ch - '0'; // перевод символа цифры в int
                    char letter = amino_acids[i + 1];
                    // Добавляем count - 1 букв, так как одна буква добавится на следующем шаге цикла
                    for (int j = 1; j < count; j++)
                    {
                        decoded += letter;
                    }
                }
                else
                {
                    decoded += ch;
                }
            }
            return decoded;
        }

        static List<Command> ReadCommands(string filename)
        {
            List<Command> commands = new List<Command>();
            if (!File.Exists(filename)) return commands;

            foreach (var line in File.ReadLines(filename))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] parts = line.Split('\t');

                Command command;
                command.name = parts[0].Trim();
                command.parameter1 = parts.Length > 1 ? parts[1].Trim() : string.Empty;
                command.parameter2 = parts.Length > 2 ? parts[2].Trim() : string.Empty;
                commands.Add(command);
            }
            return commands;
        }

        static List<Protein> ReadData(string filename)
        {
            List<Protein> data = new List<Protein>();
            if (!File.Exists(filename)) return data;

            foreach (var line in File.ReadLines(filename))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] parts = line.Split('\t');
                if (parts.Length < 3) continue;

                Protein protein;
                protein.name = parts[0].Trim();
                protein.organism = parts[1].Trim();
                // Сразу декодируем последовательность при чтении из файла!
                protein.amino_acids = Decoding(parts[2].Trim());
                data.Add(protein);
            }
            return data;
        }

        static void CommandHandler(List<Protein> proteins, List<Command> commands, string outputFilename)
        {
            // Открываем существующий файл genedata.X.txt на перезапись
            using (StreamWriter writer = new StreamWriter(outputFilename, false)) 
            {
                // Заголовок выходного файла
                writer.WriteLine("Округ Максим"); 
                writer.WriteLine("Genetic Searching");
                

                for (int i = 0; i < commands.Count; i++)
                {
                    Command cmd = commands[i];
                    string num = (i + 1).ToString("D3"); // Форматирование в 001, 002 и т.д.

                    writer.WriteLine("--------------------------------------------------------------------------");

                    if (cmd.name == "search")
                    {
                        // Текст поиска в командах тоже может быть в RLE (например FK3I), декодируем его
                        string searchPattern = Decoding(cmd.parameter1);
                        writer.WriteLine($"{num}   search   {searchPattern}");
                        writer.WriteLine("organism\t\t\t\tprotein");

                        bool found = false;
                        foreach (var p in proteins)
                        {
                            if (p.amino_acids.Contains(searchPattern))
                            {
                                writer.WriteLine($"{p.organism}\t\t{p.name}");
                                found = true;
                            }
                        }
                        if (!found)
                        {
                            writer.WriteLine("NOT FOUND");
                        }
                    }
                    else if (cmd.name == "diff")
                    {
                        writer.WriteLine($"{num}   diff   {cmd.parameter1}   {cmd.parameter2}");
                        writer.WriteLine("amino-acids difference:");

                        Protein? p1 = proteins.Any(p => p.name == cmd.parameter1) ? proteins.First(p => p.name == cmd.parameter1) : (Protein?)null;
                        Protein? p2 = proteins.Any(p => p.name == cmd.parameter2) ? proteins.First(p => p.name == cmd.parameter2) : (Protein?)null;

                        if (p1 == null || p2 == null)
                        {
                            string missing = "MISSING:";
                            if (p1 == null) missing += " " + cmd.parameter1;
                            if (p2 == null) missing += " " + cmd.parameter2;
                            writer.WriteLine(missing);
                        }
                        else
                        {
                            string s1 = p1.Value.amino_acids;
                            string s2 = p2.Value.amino_acids;
                            int diffCount = 0;

                            int minLength = Math.Min(s1.Length, s2.Length);
                            int maxLength = Math.Max(s1.Length, s2.Length);

                            // Считаем различия в общей длине
                            for (int j = 0; j < minLength; j++)
                            {
                                if (s1[j] != s2[j]) diffCount++;
                            }
                            // Добавляем остаток длины как различия
                            diffCount += (maxLength - minLength);

                            writer.WriteLine(diffCount);
                        }
                    }
                    else if (cmd.name == "mode")
                    {
                        writer.WriteLine($"{num}   mode   {cmd.parameter1}");
                        writer.WriteLine("amino-acid occurs:");

                        Protein? p = proteins.Any(prot => prot.name == cmd.parameter1) ? proteins.First(prot => prot.name == cmd.parameter1) : (Protein?)null;

                        if (p == null)
                        {
                            writer.WriteLine($"MISSING: {cmd.parameter1}");
                        }
                        else
                        {
                            string seq = p.Value.amino_acids;
                            
                            // Считаем частоту каждой буквы
                            Dictionary<char, int> counts = new Dictionary<char, int>();
                            foreach (char ch in seq)
                            {
                                if (counts.ContainsKey(ch)) counts[ch]++;
                                else counts[ch] = 1;
                            }

                            // Ищем максимум с сортировкой по алфавиту при равенстве
                            char bestChar = 'A';
                            int maxOccurs = 0;

                            foreach (var pair in counts.OrderBy(pair => pair.Key))
                            {
                                if (pair.Value > maxOccurs)
                                {
                                    maxOccurs = pair.Value;
                                    bestChar = pair.Key;
                                }
                            }

                            writer.WriteLine($"{bestChar}          {maxOccurs}");
                        }
                    }
                }
                writer.WriteLine("--------------------------------------------------------------------------");
            }
        }

        static void Main(string[] args)
        {
            // Определяем базовую папку проекта, где лежат файлы данных
            string targetDirectory = Directory.GetCurrentDirectory();
            
            // Если программа запущена из папки сборки, поднимаемся выше к корню проекта
            for (int i = 0; i < 4; i++)
            {
                if (Directory.GetFiles(targetDirectory, "commands.*.txt").Length > 0)
                {
                    break;
                }
                string parent = Directory.GetParent(targetDirectory)?.FullName;
                if (parent != null) targetDirectory = parent;
                else break;
            }

            // Ищем все файлы команд вида "commands.X.txt"
            string[] commandFiles = Directory.GetFiles(targetDirectory, "commands.*.txt");

            if (commandFiles.Length == 0)
            {
                Console.WriteLine($"Файлы 'commands.X.txt' не найдены: {targetDirectory}");
                return;
            }

            // Сортируем файлы по имени, чтобы они шли по порядку: 0, 1, 2
            Array.Sort(commandFiles);

            foreach (string commandFile in commandFiles)
            {
                string fileNameOnly = Path.GetFileName(commandFile);

                // Извлекаем номер/индекс теста из названия файла команд
                Match match = Regex.Match(fileNameOnly, @"commands\.(.+?)\.txt");
                if (match.Success)
                {
                    string index = match.Groups[1].Value;
                    string sequenceFile = Path.Combine(targetDirectory, $"sequences.{index}.txt");
                    string outputFile = Path.Combine(targetDirectory, $"genedata.{index}.txt");
                    Console.WriteLine($"\n=== Обработка набора файлов №{index} ===");
                    if (File.Exists(sequenceFile))
                    {
                        
List<Protein> data = ReadData(sequenceFile);
List<Command> commands = ReadCommands(commandFile);
                    
                CommandHandler(data, commands, outputFile);
Console.WriteLine($"Успешно! Результаты перезаписаны в: {Path.GetFileName(outputFile)}");
                    }
                    else
                    {
                        
                    Console.WriteLine($"Предупреждение: Файл sequences.{index}.txt не найден.");
                    }
                }
            }
            Console.WriteLine("\nВсе доступные тесты успешно обработаны!");
        }
    }
}