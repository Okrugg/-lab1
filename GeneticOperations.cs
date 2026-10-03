using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GeneticSearch
{
    public static class GeneticOperations
    {
        public static void ExecuteSearch(List<Protein> proteins, Command cmd, string num, StreamWriter writer)
        {
            string searchPattern = Decoder.Decoding(cmd.parameter1);
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

        public static void ExecuteDiff(List<Protein> proteins, Command cmd, string num, StreamWriter writer)
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

                for (int j = 0; j < minLength; j++)
                {
                    if (s1[j] != s2[j]) diffCount++;
                }
                diffCount += (maxLength - minLength);

                writer.WriteLine(diffCount);
            }
        }

        public static void ExecuteMode(List<Protein> proteins, Command cmd, string num, StreamWriter writer)
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
                
                Dictionary<char, int> counts = new Dictionary<char, int>();
                foreach (char ch in seq)
                {
                    if (counts.ContainsKey(ch)) counts[ch]++;
                    else counts[ch] = 1;
                }

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
}
