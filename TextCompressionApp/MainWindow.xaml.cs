using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace TextCompressionApp
{
    public partial class MainWindow : Window
    {
        private HuffmanCoding _huffmanCoding;
        private RunLengthCoding _runLengthCoding;

        public MainWindow()
        {
            InitializeComponent();
            _huffmanCoding = new HuffmanCoding();
            _runLengthCoding = new RunLengthCoding();
        }

        // Open File Button Click
        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                string fileContent = File.ReadAllText(filePath);

                // Display file content in both tabs
                HuffmanInputText.Text = fileContent;
                RunLengthInputText.Text = fileContent;

                StatusText.Text = $"File loaded: {filePath}";
            }
        }

        // Save Results Button Click
        private void SaveResults_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string results = "";

                // Collect results from Huffman Coding
                results += "Huffman Coding Results:\n";
                foreach (var item in HuffmanResultsGrid.Items)
                {
                    results += $"{item}\n";
                }

                // Collect results from Run-Length Coding
                results += "\nRun-Length Coding Results:\n";
                foreach (var item in RunLengthResultsGrid.Items)
                {
                    results += $"{item}\n";
                }

                // Save results to file
                File.WriteAllText(saveFileDialog.FileName, results);

                StatusText.Text = $"Results saved: {saveFileDialog.FileName}";
            }
        }

        // Exit Button Click
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Huffman Compress Button Click
        private void HuffmanCompress_Click(object sender, RoutedEventArgs e)
        {
            string inputText = HuffmanInputText.Text;
            if (string.IsNullOrEmpty(inputText))
            {
                MessageBox.Show("Please load a file first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Measure compression time
            var stopwatch = Stopwatch.StartNew();
            string compressedText = _huffmanCoding.Compress(inputText);
            stopwatch.Stop();

            // Create a list of ResultItem objects to bind to the grid
            List<ResultItem> results = new List<ResultItem>
            {
                new ResultItem { Metric = "Compressed Text", Value = compressedText },
                new ResultItem { Metric = "Compression Ratio", Value = (double)compressedText.Length / inputText.Length },
                new ResultItem { Metric = "Entropy (Before)", Value = CalculateEntropy(inputText) },
                new ResultItem { Metric = "Entropy (After)", Value = CalculateEntropy(compressedText) },
                new ResultItem { Metric = "Computation Time (ms)", Value = stopwatch.ElapsedMilliseconds }
            };

            HuffmanResultsGrid.ItemsSource = results;

            StatusText.Text = "Huffman Compression completed.";
        }

        // Huffman Decompress Button Click
        private void HuffmanDecompress_Click(object sender, RoutedEventArgs e)
        {
            string compressedText = HuffmanInputText.Text;
            if (string.IsNullOrEmpty(compressedText))
            {
                MessageBox.Show("No compressed text found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Measure decompression time
            var stopwatch = Stopwatch.StartNew();
            string decompressedText = _huffmanCoding.Decompress(compressedText);
            stopwatch.Stop();

            // Create a list of ResultItem objects to bind to the grid
            List<ResultItem> results = new List<ResultItem>
            {
                new ResultItem { Metric = "Decompressed Text", Value = decompressedText },
                new ResultItem { Metric = "Computation Time (ms)", Value = stopwatch.ElapsedMilliseconds }
            };

            HuffmanResultsGrid.ItemsSource = results;

            StatusText.Text = "Huffman Decompression completed.";
        }

        // Run-Length Compress Button Click
        private void RunLengthCompress_Click(object sender, RoutedEventArgs e)
        {
            string inputText = RunLengthInputText.Text;
            if (string.IsNullOrEmpty(inputText))
            {
                MessageBox.Show("Please load a file first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Measure compression time
            var stopwatch = Stopwatch.StartNew();
            string compressedText = _runLengthCoding.Compress(inputText);
            stopwatch.Stop();

            // Create a list of ResultItem objects to bind to the grid
            List<ResultItem> results = new List<ResultItem>
            {
                new ResultItem { Metric = "Compressed Text", Value = compressedText },
                new ResultItem { Metric = "Compression Ratio", Value = (double)compressedText.Length / inputText.Length },
                new ResultItem { Metric = "Entropy (Before)", Value = CalculateEntropy(inputText) },
                new ResultItem { Metric = "Entropy (After)", Value = CalculateEntropy(compressedText) },
                new ResultItem { Metric = "Computation Time (ms)", Value = stopwatch.ElapsedMilliseconds }
            };

            RunLengthResultsGrid.ItemsSource = results;

            StatusText.Text = "Run-Length Compression completed.";
        }

        // Run-Length Decompress Button Click
        private void RunLengthDecompress_Click(object sender, RoutedEventArgs e)
        {
            string compressedText = RunLengthInputText.Text;
            if (string.IsNullOrEmpty(compressedText))
            {
                MessageBox.Show("No compressed text found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Measure decompression time
            var stopwatch = Stopwatch.StartNew();
            string decompressedText = _runLengthCoding.Decompress(compressedText);
            stopwatch.Stop();

            // Create a list of ResultItem objects to bind to the grid
            List<ResultItem> results = new List<ResultItem>
            {
                new ResultItem { Metric = "Decompressed Text", Value = decompressedText },
                new ResultItem { Metric = "Computation Time (ms)", Value = stopwatch.ElapsedMilliseconds }
            };

            RunLengthResultsGrid.ItemsSource = results;

            StatusText.Text = "Run-Length Decompression completed.";
        }

        // Calculate entropy of the input text
        private double CalculateEntropy(string inputText)
        {
            if (string.IsNullOrEmpty(inputText))
                return 0;

            var frequencies = inputText.GroupBy(c => c)
                                       .ToDictionary(g => g.Key, g => (double)g.Count() / inputText.Length);

            return -frequencies.Values.Sum(p => p * Math.Log(p, 2));
        }

        // Define the ResultItem class to hold the metric and value
        public class ResultItem
        {
            public string Metric { get; set; }
            public object Value { get; set; }
        }
    }
}
