using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace TextCompressionApp
{
    public class RunLengthEncoding
    {
        private Dictionary<char, int> _frequencies;
        private double _averageWordLength;
        private const int MAX_RUN_LENGTH = 255; // Maximum run length to ensure byte storage

        public RunLengthEncoding()
        {
            _frequencies = new Dictionary<char, int>();
        }

        public byte[] Compress(string inputText)
        {
            if (string.IsNullOrEmpty(inputText))
                return Array.Empty<byte>();

            CalculateFrequencies(inputText);
            CalculateAverageWordLength();

            var compressed = new List<byte>();
            int count = 1;
            char currentChar = inputText[0];

            // Process the input text
            for (int i = 1; i < inputText.Length; i++)
            {
                if (inputText[i] == currentChar && count < MAX_RUN_LENGTH)
                {
                    count++;
                }
                else
                {
                    // Store character and count
                    compressed.Add((byte)currentChar);
                    compressed.Add((byte)count);

                    currentChar = inputText[i];
                    count = 1;
                }
            }

            // Handle the last run
            compressed.Add((byte)currentChar);
            compressed.Add((byte)count);

            return compressed.ToArray();
        }

        public string Decompress(byte[] compressedData)
        {
            if (compressedData == null || compressedData.Length == 0 || compressedData.Length % 2 != 0)
                return string.Empty;

            var decompressed = new StringBuilder();

            // Process pairs of bytes (character and count)
            for (int i = 0; i < compressedData.Length; i += 2)
            {
                char character = (char)compressedData[i];
                int count = compressedData[i + 1];

                decompressed.Append(character, count);
            }

            return decompressed.ToString();
        }

        public byte[] CompressIntegers(int[] numbers)
        {
            if (numbers == null || numbers.Length == 0)
                return Array.Empty<byte>();

            var compressed = new List<byte>();
            int count = 1;
            int currentNumber = numbers[0];

            // Store the type of data (1 for integers)
            compressed.Add(1);

            for (int i = 1; i < numbers.Length; i++)
            {
                if (numbers[i] == currentNumber && count < MAX_RUN_LENGTH)
                {
                    count++;
                }
                else
                {
                    // Store the integer value (4 bytes) and count
                    byte[] numberBytes = BitConverter.GetBytes(currentNumber);
                    compressed.AddRange(numberBytes);
                    compressed.Add((byte)count);

                    currentNumber = numbers[i];
                    count = 1;
                }
            }

            // Handle the last run
            byte[] lastNumberBytes = BitConverter.GetBytes(currentNumber);
            compressed.AddRange(lastNumberBytes);
            compressed.Add((byte)count);

            return compressed.ToArray();
        }

        public int[] DecompressIntegers(byte[] compressedData)
        {
            if (compressedData == null || compressedData.Length < 6 || compressedData[0] != 1)
                return Array.Empty<int>();

            var decompressed = new List<int>();

            // Skip the type identifier byte
            for (int i = 1; i < compressedData.Length; i += 5)
            {
                int number = BitConverter.ToInt32(compressedData, i);
                int count = compressedData[i + 4];

                for (int j = 0; j < count; j++)
                {
                    decompressed.Add(number);
                }
            }

            return decompressed.ToArray();
        }

        private void CalculateFrequencies(string text)
        {
            _frequencies.Clear();
            foreach (char c in text)
            {
                if (!_frequencies.ContainsKey(c))
                    _frequencies[c] = 0;
                _frequencies[c]++;
            }
        }

        public double GetAverageWordLength()
        {
            return _averageWordLength;
        }

        private void CalculateAverageWordLength()
        {
            _averageWordLength = 0;
            int totalCharacters = _frequencies.Values.Sum();

            foreach (var kvp in _frequencies)
            {
                int runLength = kvp.Value;
                // For RLE, each character is encoded with its count
                // So word length is 16 bits (8 for char + 8 for count)
                double probability = (double)runLength / totalCharacters;
                _averageWordLength += 16 * probability;
            }
        }

        public Dictionary<char, double> GetProbabilities()
        {
            int totalCharacters = _frequencies.Values.Sum();
            return _frequencies.ToDictionary(
                kvp => kvp.Key,
                kvp => (double)kvp.Value / totalCharacters
            );
        }

        public int GetMaxRunLength()
        {
            return _frequencies.Values.Max();
        }

        public Dictionary<char, int> GetRunLengths()
        {
            return new Dictionary<char, int>(_frequencies);
        }

        public class CompressionStatistics
        {
            public int OriginalSize { get; set; }
            public int CompressedSize { get; set; }
            public double CompressionRatio { get; set; }
            public Dictionary<char, int> RunLengths { get; set; }
            public Dictionary<char, double> Probabilities { get; set; }
            public double AverageWordLength { get; set; }
        }

        public CompressionStatistics GetStatistics(string originalText, byte[] compressedData)
        {
            return new CompressionStatistics
            {
                OriginalSize = originalText.Length,
                CompressedSize = compressedData.Length,
                CompressionRatio = (double)compressedData.Length / originalText.Length,
                RunLengths = GetRunLengths(),
                Probabilities = GetProbabilities(),
                AverageWordLength = GetAverageWordLength()
            };
        }
    }
}