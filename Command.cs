using System;
using System.Collections.Generic;
using System.IO;

namespace GeneticSearch
{
    public struct Command  
    {
        public string name;        
        public string parameter1;  
        public string parameter2;  

        public static List<Command> ReadFromFile(string filename)
        {
            List<Command> commands = new List<Command>();
            if (!File.Exists(filename)) return commands;

            foreach (var line in File.ReadLines(filename))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] parts = line.Split('\t');

                if (parts.Length > 3) 
                {
                    Console.WriteLine($"Предупреждение: Строка пропущена (лишний параметр): {line.Trim()}");
                    continue; 
                }

                Command command;
                command.name = parts[0].Trim();
                command.parameter1 = parts.Length > 1 ? parts[1].Trim() : string.Empty;
                command.parameter2 = parts.Length > 2 ? parts[2].Trim() : string.Empty;
                commands.Add(command);
            }
            return commands;
        }
    }
}
