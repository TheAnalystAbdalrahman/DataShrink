using System;
using System.Text;

namespace TextCompressionApp
{
    public class RunLengthCoding
    {
        // Compress the input text
        public string Compress(string inputText)
        {
            if (string.IsNullOrEmpty(inputText))
                return string.Empty;

            var compressedText = new StringBuilder();
            int count = 1;

            for (int i = 1; i < inputText.Length; i++)
            {
                if (inputText[i] == inputText[i - 1])
                {
                    count++;
                }
                else
                {
                    compressedText.Append(inputText[i - 1]);
                    compressedText.Append(count);
                    count = 1;
                }
            }

            // Append the last character and its count
            compressedText.Append(inputText[inputText.Length - 1]);
            compressedText.Append(count);

            return compressedText.ToString();
        }

        // Decompress the compressed text
        public string Decompress(string compressedText)
        {
            if (string.IsNullOrEmpty(compressedText))
                return string.Empty;

            var decompressedText = new StringBuilder();

            for (int i = 0; i < compressedText.Length; i += 2)
            {
                char character = compressedText[i];
                if (i + 1 >= compressedText.Length || !char.IsDigit(compressedText[i + 1]))
                {
                    throw new FormatException("Invalid format in compressed text.");
                }

                int count = int.Parse(compressedText[i + 1].ToString());
                decompressedText.Append(character, count);
            }

            return decompressedText.ToString();
        }
    }
}