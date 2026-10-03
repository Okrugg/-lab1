namespace GeneticSearch
{
    public static class Decoder
    {
        public static string Decoding(string amino_acids)
        {
            if (string.IsNullOrEmpty(amino_acids)) return string.Empty;

            string decoded = string.Empty;
            for (int i = 0; i < amino_acids.Length; i++)
            {
                char ch = amino_acids[i];
                if (char.IsDigit(ch))
                {
                    int count = ch - '0'; 
                    char letter = amino_acids[i + 1];
                   
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
    }
}
