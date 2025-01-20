using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextCompressionApp
{
    public class HuffmanCoding
    {
        private class HuffmanNode
        {
            public char Character { get; set; }
            public int Frequency { get; set; }
            public HuffmanNode Left { get; set; }
            public HuffmanNode Right { get; set; }
            public string Code { get; set; }
        }

        private HuffmanNode _root;
        private Dictionary<char, string> _huffmanCodes;
        private Dictionary<char, int> _frequencies;
        private double _averageWordLength;

        public HuffmanCoding()
        {
            _huffmanCodes = new Dictionary<char, string>();
            _frequencies = new Dictionary<char, int>();
        }

        public byte[] Compress(string inputText)
        {
            if (string.IsNullOrEmpty(inputText))
                return Array.Empty<byte>();

            // Calculate frequencies and probabilities
            CalculateFrequencies(inputText);

            // Build Huffman tree
            _root = BuildHuffmanTree();

            // Generate Huffman codes
            _huffmanCodes.Clear();
            GenerateHuffmanCodes(_root, "");

            // Calculate average word length
            CalculateAverageWordLength();

            // Encode text
            var encodedText = EncodeText(inputText);

            // Add padding and metadata
            var (paddedText, paddingLength) = AddPadding(encodedText);

            // Convert to bytes with metadata
            return CreateCompressedBytes(paddedText, paddingLength);
        }

        public string Decompress(byte[] compressedData)
        {
            if (_root == null || compressedData == null || compressedData.Length < 2)
                return string.Empty;

            // Extract metadata and encoded text
            int paddingLength = compressedData[0];
            var encodedText = GetBinaryString(compressedData.Skip(1).ToArray());

            // Remove padding
            encodedText = encodedText.Substring(0, encodedText.Length - paddingLength);

            // Decode text
            return DecodeText(encodedText);
        }

        public double GetAverageWordLength()
        {
            return _averageWordLength;
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

        private HuffmanNode BuildHuffmanTree()
        {
            var priorityQueue = new List<HuffmanNode>();

            // Create leaf nodes
            foreach (var kvp in _frequencies)
            {
                priorityQueue.Add(new HuffmanNode
                {
                    Character = kvp.Key,
                    Frequency = kvp.Value
                });
            }

            // Build tree
            while (priorityQueue.Count > 1)
            {
                priorityQueue = priorityQueue.OrderBy(n => n.Frequency).ToList();

                var left = priorityQueue[0];
                var right = priorityQueue[1];

                var parent = new HuffmanNode
                {
                    Character = '\0',
                    Frequency = left.Frequency + right.Frequency,
                    Left = left,
                    Right = right
                };

                priorityQueue.RemoveRange(0, 2);
                priorityQueue.Add(parent);
            }

            return priorityQueue.FirstOrDefault();
        }

        private void GenerateHuffmanCodes(HuffmanNode node, string code)
        {
            if (node == null)
                return;

            node.Code = code;

            if (node.Left == null && node.Right == null)
            {
                _huffmanCodes[node.Character] = code;
                return;
            }

            GenerateHuffmanCodes(node.Left, code + "0");
            GenerateHuffmanCodes(node.Right, code + "1");
        }

        private void CalculateAverageWordLength()
        {
            _averageWordLength = 0;
            int totalCharacters = _frequencies.Values.Sum();

            foreach (var kvp in _huffmanCodes)
            {
                char character = kvp.Key;
                string code = kvp.Value;
                double probability = (double)_frequencies[character] / totalCharacters;
                _averageWordLength += code.Length * probability;
            }
        }

        private string EncodeText(string text)
        {
            var encodedBuilder = new StringBuilder();
            foreach (char c in text)
            {
                encodedBuilder.Append(_huffmanCodes[c]);
            }
            return encodedBuilder.ToString();
        }

        private (string paddedText, int paddingLength) AddPadding(string encodedText)
        {
            int paddingLength = 8 - (encodedText.Length % 8);
            if (paddingLength == 8) paddingLength = 0;
            return (encodedText.PadRight(encodedText.Length + paddingLength, '0'), paddingLength);
        }

        private byte[] CreateCompressedBytes(string paddedText, int paddingLength)
        {
            // First byte stores padding length
            var compressedBytes = new List<byte> { (byte)paddingLength };

            // Convert binary string to bytes
            for (int i = 0; i < paddedText.Length; i += 8)
            {
                string chunk = paddedText.Substring(i, 8);
                compressedBytes.Add(Convert.ToByte(chunk, 2));
            }

            return compressedBytes.ToArray();
        }

        private string GetBinaryString(byte[] bytes)
        {
            var binary = new StringBuilder();
            foreach (byte b in bytes)
            {
                binary.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
            }
            return binary.ToString();
        }

        private string DecodeText(string encodedText)
        {
            var decodedText = new StringBuilder();
            var currentNode = _root;

            foreach (char bit in encodedText)
            {
                currentNode = (bit == '0') ? currentNode.Left : currentNode.Right;

                if (currentNode.Left == null && currentNode.Right == null)
                {
                    decodedText.Append(currentNode.Character);
                    currentNode = _root;
                }
            }

            return decodedText.ToString();
        }

        public Dictionary<char, string> GetCurrentCodes()
        {
            return new Dictionary<char, string>(_huffmanCodes);
        }

        public Dictionary<char, double> GetProbabilities()
        {
            int totalCharacters = _frequencies.Values.Sum();
            return _frequencies.ToDictionary(
                kvp => kvp.Key,
                kvp => (double)kvp.Value / totalCharacters
            );
        }
    }
}