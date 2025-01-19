using System;
using System.Collections.Generic;
using System.Linq;

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
        }

        private HuffmanNode _root;
        private Dictionary<char, string> _huffmanCodes;

        public HuffmanCoding()
        {
            _huffmanCodes = new Dictionary<char, string>();
        }

        // Compress the input text
        public string Compress(string inputText)
        {
            if (string.IsNullOrEmpty(inputText))
                return string.Empty;

            // Calculate character frequencies
            var frequencies = inputText.GroupBy(c => c)
                                       .ToDictionary(g => g.Key, g => g.Count());

            // Build Huffman tree
            _root = BuildHuffmanTree(frequencies);

            // Generate Huffman codes
            _huffmanCodes.Clear();
            GenerateCodes(_root, "");

            // Encode the input text
            var compressedText = string.Join("", inputText.Select(c => _huffmanCodes[c]));

            return compressedText;
        }

        // Decompress the compressed text
        public string Decompress(string compressedText)
        {
            if (_root == null || string.IsNullOrEmpty(compressedText))
                return string.Empty;

            var decompressedText = "";
            var currentNode = _root;

            foreach (var bit in compressedText)
            {
                currentNode = (bit == '0') ? currentNode.Left : currentNode.Right;

                if (currentNode.Left == null && currentNode.Right == null)
                {
                    decompressedText += currentNode.Character;
                    currentNode = _root;
                }
            }

            return decompressedText;
        }

        // Build Huffman tree
        private HuffmanNode BuildHuffmanTree(Dictionary<char, int> frequencies)
        {
            var priorityQueue = new List<HuffmanNode>();

            foreach (var kvp in frequencies)
            {
                priorityQueue.Add(new HuffmanNode
                {
                    Character = kvp.Key,
                    Frequency = kvp.Value
                });
            }

            while (priorityQueue.Count > 1)
            {
                priorityQueue = priorityQueue.OrderBy(n => n.Frequency).ToList();

                var left = priorityQueue[0];
                var right = priorityQueue[1];

                var parent = new HuffmanNode
                {
                    Frequency = left.Frequency + right.Frequency,
                    Left = left,
                    Right = right
                };

                priorityQueue.RemoveRange(0, 2);
                priorityQueue.Add(parent);
            }

            return priorityQueue.FirstOrDefault();
        }

        // Generate Huffman codes
        private void GenerateCodes(HuffmanNode node, string code)
        {
            if (node == null)
                return;

            if (node.Left == null && node.Right == null)
            {
                _huffmanCodes[node.Character] = code;
                return;
            }

            GenerateCodes(node.Left, code + "0");
            GenerateCodes(node.Right, code + "1");
        }
    }
}