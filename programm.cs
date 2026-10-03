using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace GeneticSearch
{
    class Program
    {
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
                protein.amino_acids = Decoder.Decoding(parts[2].Trim());
                data.Add(protein);
            }
            return data;
        }

        static void CommandHandler(List<Protein> proteins, List<Command> commands, string outputFilename)
        {
            using (StreamWriter writer = new StreamWriter(outputFilename, false)) 
            {
                writer.WriteLine("Округ Максим"); 
                writer.WriteLine("Genetic Searching");

                for (int i = 0; i < commands.Count; i++)
                {
                    Command cmd = commands[i];
                    string num = (i + 1).ToString("D3"); 

                    writer.WriteLine("--------------------------------------------------------------------------");

                    if (cmd.name == "search")
                    {
                        GeneticOperations.ExecuteSearch(proteins, cmd, num, writer);
                    }
                    else if (cmd.name == "diff")
                    {
                        GeneticOperations.ExecuteDiff(proteins, cmd, num, writer);
                    }
                    else if (cmd.name == "mode")
                    {
                        GeneticOperations.ExecuteMode(proteins, cmd, num, writer);
                    }
                }
                writer.WriteLine("--------------------------------------------------------------------------");
            }
        }

                static void Main(string[] args)
        {
            string targetDirectory = Directory.GetCurrentDirectory();
            
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
            Console.WriteLine("=== ЗАПУСК ТЕСТА: ПРОВЕРКА НА ЛИШНИЕ ПАРАМЕТРЫ (БОЛЬШЕ 3 ЭЛЕМЕНТОВ) ===");
            string testFilePath = Path.Combine(targetDirectory, "commands.test_limit.txt");

            File.WriteAllLines(testFilePath, new string[]
            {
                "search\tAAAA",                           
                "diff\tProtein1\tProtein2",               
                "mode\tProtein1\tExtra1\tExtra2",         
                "search\tBBBB\tExtra1\tExtra2\tExtra3",  
                "mode\tProtein2"                         
            });

            List<Command> testCommands = Command.ReadFromFile(testFilePath);

            Console.WriteLine($"\nИтог теста: Из 5 строк успешно загружено команд: {testCommands.Count} (Должно быть 3).");
            Console.WriteLine("==========================================================================\n");
            
            if (File.Exists(testFilePath)) File.Delete(testFilePath);
           
            string[] commandFiles = Directory.GetFiles(targetDirectory, "commands.*.txt");

            if (commandFiles.Length == 0)
            {
                Console.WriteLine($"Файлы 'commands.X.txt' не найдены: {targetDirectory}");
                return;
            }

            Array.Sort(commandFiles);

            foreach (string commandFile in commandFiles)
            {
                string fileNameOnly = Path.GetFileName(commandFile);

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
                        List<Command> commands = Command.ReadFromFile(commandFile);
                                            
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
