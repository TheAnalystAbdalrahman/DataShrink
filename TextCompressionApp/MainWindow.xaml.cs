using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Text.RegularExpressions;

namespace TextCompressionApp
{
    public partial class MainWindow : Window
    {
        private HuffmanCoding _huffmanCoding;
        private RunLengthEncoding _runLengthEncoding;
        private string _originalText;
        private byte[] _huffmanCompressed;
        private byte[] _runLengthCompressed;
        private CompressionMetrics _huffmanMetrics;
        private CompressionMetrics _runLengthMetrics;

        public MainWindow()
        {
            InitializeComponent();
            _huffmanCoding = new HuffmanCoding();
            _runLengthEncoding = new RunLengthEncoding();
            CompressionProgress.Visibility = Visibility.Visible;
            CompressionProgress.Value = 0;
        }

        private void AnalyzeFile_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(InputText.Text))
            {
                MessageBox.Show("Please load a file first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string text = InputText.Text;
            int length = text.Length;

            // Determine file size category
            string sizeCategory;
            if (length < 500) sizeCategory = "Very Small";
            else if (length <= 1000) sizeCategory = "Small";
            else if (length <= 5000) sizeCategory = "Medium";
            else if (length <= 10000) sizeCategory = "Large";
            else sizeCategory = "Very Large";

            FileSizeRangeText.Text = $"File Size: {length} characters ({sizeCategory})";

            // Analyze integers if present
            var numbers = Regex.Matches(text, @"-?\d+")
                              .Cast<Match>()
                              .Select(m => long.Parse(m.Value))
                              .ToList();

            if (numbers.Any())
            {
                long min = numbers.Min();
                long max = numbers.Max();
                IntegerRangeText.Text = $"Integer Range: {min} to {max}";
            }
            else
            {
                IntegerRangeText.Text = "No integers found in text";
            }

            StatusText.Text = "File analysis completed.";
        }

        private void RunCompressionTests_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(InputText.Text))
            {
                MessageBox.Show("Please load and analyze a file first.", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _originalText = InputText.Text;
            CompressionProgress.Value = 20;

            // Run Huffman compression
            var huffmanStopwatch = Stopwatch.StartNew();
            _huffmanCompressed = _huffmanCoding.Compress(_originalText);
            huffmanStopwatch.Stop();

            CompressionProgress.Value = 40;

            // Run Run-Length compression
            var runLengthStopwatch = Stopwatch.StartNew();
            _runLengthCompressed = _runLengthEncoding.Compress(_originalText);
            runLengthStopwatch.Stop();

            CompressionProgress.Value = 60;

            // Calculate metrics for both methods
            _huffmanMetrics = CalculateMetrics(_originalText, _huffmanCompressed,
                                             _huffmanCoding.GetAverageWordLength(),
                                             huffmanStopwatch.ElapsedMilliseconds);

            _runLengthMetrics = CalculateMetrics(_originalText, _runLengthCompressed,
                                                _runLengthEncoding.GetAverageWordLength(),
                                                runLengthStopwatch.ElapsedMilliseconds);

            CompressionProgress.Value = 80;

            // Update results grids
            UpdateResultsGrids();

            CompressionProgress.Value = 100;
            StatusText.Text = "Compression tests completed.";
        }

        private CompressionMetrics CalculateMetrics(string original, byte[] compressed,
                                                  double avgWordLength, long computationTime)
        {
            return new CompressionMetrics
            {
                OriginalSize = original.Length,
                CompressedSize = compressed.Length,
                CompressionRatio = (double)compressed.Length / original.Length,
                EntropyBefore = CalculateEntropy(original),
                EntropyAfter = CalculateEntropy(Convert.ToBase64String(compressed)),
                AverageWordLength = avgWordLength,
                ComputationTime = computationTime
            };
        }

        private void UpdateResultsGrids()
        {
            var huffmanResults = new List<ResultItem>
            {
                new ResultItem("Original Size (bytes)", _huffmanMetrics.OriginalSize),
                new ResultItem("Compressed Size (bytes)", _huffmanMetrics.CompressedSize),
                new ResultItem("Compression Ratio", _huffmanMetrics.CompressionRatio.ToString("F4")),
                new ResultItem("Entropy Before", _huffmanMetrics.EntropyBefore.ToString("F4")),
                new ResultItem("Entropy After", _huffmanMetrics.EntropyAfter.ToString("F4")),
                new ResultItem("Average Word Length", _huffmanMetrics.AverageWordLength.ToString("F4")),
                new ResultItem("Computation Time (ms)", _huffmanMetrics.ComputationTime)
            };

            var runLengthResults = new List<ResultItem>
            {
                new ResultItem("Original Size (bytes)", _runLengthMetrics.OriginalSize),
                new ResultItem("Compressed Size (bytes)", _runLengthMetrics.CompressedSize),
                new ResultItem("Compression Ratio", _runLengthMetrics.CompressionRatio.ToString("F4")),
                new ResultItem("Entropy Before", _runLengthMetrics.EntropyBefore.ToString("F4")),
                new ResultItem("Entropy After", _runLengthMetrics.EntropyAfter.ToString("F4")),
                new ResultItem("Average Word Length", _runLengthMetrics.AverageWordLength.ToString("F4")),
                new ResultItem("Computation Time (ms)", _runLengthMetrics.ComputationTime)
            };

            HuffmanResultsGrid.ItemsSource = huffmanResults;
            RunLengthResultsGrid.ItemsSource = runLengthResults;
        }

        private void CompareResults_Click(object sender, RoutedEventArgs e)
        {
            if (_huffmanMetrics == null || _runLengthMetrics == null)
            {
                MessageBox.Show("Please run compression tests first.", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var comparison = new StringBuilder();
            comparison.AppendLine("Compression Comparison Results:");
            comparison.AppendLine();
            comparison.AppendLine($"Original File Size: {_originalText.Length} bytes");
            comparison.AppendLine();
            comparison.AppendLine("Huffman Coding:");
            comparison.AppendLine($"- Compressed Size: {_huffmanMetrics.CompressedSize} bytes");
            comparison.AppendLine($"- Compression Ratio: {_huffmanMetrics.CompressionRatio:F4}");
            comparison.AppendLine($"- Computation Time: {_huffmanMetrics.ComputationTime}ms");
            comparison.AppendLine();
            comparison.AppendLine("Run-Length Coding:");
            comparison.AppendLine($"- Compressed Size: {_runLengthMetrics.CompressedSize} bytes");
            comparison.AppendLine($"- Compression Ratio: {_runLengthMetrics.CompressionRatio:F4}");
            comparison.AppendLine($"- Computation Time: {_runLengthMetrics.ComputationTime}ms");
            comparison.AppendLine();
            comparison.AppendLine("Winner by Category:");
            comparison.AppendLine($"- Size: {(_huffmanMetrics.CompressedSize < _runLengthMetrics.CompressedSize ? "Huffman" : "Run-Length")}");
            comparison.AppendLine($"- Speed: {(_huffmanMetrics.ComputationTime < _runLengthMetrics.ComputationTime ? "Huffman" : "Run-Length")}");

            MetricsText.Text = comparison.ToString();
        }

        private void VerifyHuffman_Click(object sender, RoutedEventArgs e)
        {
            if (_huffmanCompressed == null || string.IsNullOrEmpty(_originalText))
            {
                MessageBox.Show("Please run compression tests first.", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string decompressed = _huffmanCoding.Decompress(_huffmanCompressed);
            VerifyOriginalText.Text = _originalText;
            VerifyDecompressedText.Text = decompressed;

            bool isMatch = _originalText.Equals(decompressed);
            MessageBox.Show($"Verification Result: {(isMatch ? "Success" : "Failed")}\n" +
                          $"Original Length: {_originalText.Length}\n" +
                          $"Decompressed Length: {decompressed.Length}",
                          "Huffman Verification",
                          MessageBoxButton.OK,
                          isMatch ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }

        private void VerifyRunLength_Click(object sender, RoutedEventArgs e)
        {
            if (_runLengthCompressed == null || string.IsNullOrEmpty(_originalText))
            {
                MessageBox.Show("Please run compression tests first.", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string decompressed = _runLengthEncoding.Decompress(_runLengthCompressed);
            VerifyOriginalText.Text = _originalText;
            VerifyDecompressedText.Text = decompressed;

            bool isMatch = _originalText.Equals(decompressed);
            MessageBox.Show($"Verification Result: {(isMatch ? "Success" : "Failed")}\n" +
                          $"Original Length: {_originalText.Length}\n" +
                          $"Decompressed Length: {decompressed.Length}",
                          "Run-Length Verification",
                          MessageBoxButton.OK,
                          isMatch ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _originalText = File.ReadAllText(openFileDialog.FileName);
                    InputText.Text = _originalText;
                    StatusText.Text = $"File loaded: {openFileDialog.FileName}";

                    // Reset results
                    _huffmanCompressed = null;
                    _runLengthCompressed = null;
                    _huffmanMetrics = null;
                    _runLengthMetrics = null;
                    HuffmanResultsGrid.ItemsSource = null;
                    RunLengthResultsGrid.ItemsSource = null;
                    MetricsText.Text = string.Empty;
                    CompressionProgress.Value = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading file: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveResults_Click(object sender, RoutedEventArgs e)
        {
            if (_huffmanMetrics == null || _runLengthMetrics == null)
            {
                MessageBox.Show("No compression results to save.", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = "compression_results.txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var results = new StringBuilder();
                    results.AppendLine("Compression Analysis Results");
                    results.AppendLine("=========================");
                    results.AppendLine($"Date: {DateTime.Now}");
                    results.AppendLine($"Original File Size: {_originalText.Length} bytes");
                    results.AppendLine();

                    // Add Huffman results
                    results.AppendLine("Huffman Coding Results:");
                    results.AppendLine($"Compressed Size: {_huffmanMetrics.CompressedSize} bytes");
                    results.AppendLine($"Compression Ratio: {_huffmanMetrics.CompressionRatio:F4}");
                    results.AppendLine($"Entropy Before: {_huffmanMetrics.EntropyBefore:F4}");
                    results.AppendLine($"Entropy After: {_huffmanMetrics.EntropyAfter:F4}");
                    results.AppendLine($"Average Word Length: {_huffmanMetrics.AverageWordLength:F4}");
                    results.AppendLine($"Computation Time: {_huffmanMetrics.ComputationTime}ms");
                    results.AppendLine();

                    // Add Run-Length results
                    results.AppendLine("Run-Length Coding Results:");
                    results.AppendLine($"Compressed Size: {_runLengthMetrics.CompressedSize} bytes");
                    results.AppendLine($"Compression Ratio: {_runLengthMetrics.CompressionRatio:F4}");
                    results.AppendLine($"Entropy Before: {_runLengthMetrics.EntropyBefore:F4}");
                    results.AppendLine($"Entropy After: {_runLengthMetrics.EntropyAfter:F4}");
                    results.AppendLine($"Average Word Length: {_runLengthMetrics.AverageWordLength:F4}");
                    results.AppendLine($"Computation Time: {_runLengthMetrics.ComputationTime}ms");

                    File.WriteAllText(saveFileDialog.FileName, results.ToString());
                    StatusText.Text = $"Results saved to: {saveFileDialog.FileName}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving results: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveCompressed_Click(object sender, RoutedEventArgs e)
        {
            if (_huffmanCompressed == null && _runLengthCompressed == null)
            {
                MessageBox.Show("No compressed data to save.", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Compressed files (*.comp)|*.comp|All files (*.*)|*.*",
                FileName = "compressed_data.comp"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Save both compressed versions in a simple format
                    using (var writer = new BinaryWriter(File.Open(saveFileDialog.FileName, FileMode.Create)))
                    {
                        // Write Huffman data
                        writer.Write(_huffmanCompressed?.Length ?? 0);
                        if (_huffmanCompressed != null)
                            writer.Write(_huffmanCompressed);

                        // Write Run-Length data
                        writer.Write(_runLengthCompressed?.Length ?? 0);
                        if (_runLengthCompressed != null)
                            writer.Write(_runLengthCompressed);
                    }

                    StatusText.Text = $"Compressed data saved to: {saveFileDialog.FileName}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving compressed data: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveDecompressed_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(VerifyDecompressedText.Text))
            {
                MessageBox.Show("No decompressed text to save.", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = "decompressed_text.txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, VerifyDecompressedText.Text);
                    StatusText.Text = $"Decompressed text saved to: {saveFileDialog.FileName}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving decompressed text: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveDetailedReport_Click(object sender, RoutedEventArgs e)
        {
            if (_huffmanMetrics == null || _runLengthMetrics == null)
            {
                MessageBox.Show("Please run compression tests first.", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "HTML files (*.html)|*.html|All files (*.*)|*.*",
                FileName = "detailed_report.html"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var report = new StringBuilder();
                    report.AppendLine("<!DOCTYPE html>");
                    report.AppendLine("<html><head><title>Compression Analysis Report</title>");
                    report.AppendLine("<style>");
                    report.AppendLine("body { font-family: Arial, sans-serif; margin: 40px; }");
                    report.AppendLine("table { border-collapse: collapse; width: 100%; }");
                    report.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
                    report.AppendLine("th { background-color: #f2f2f2; }");
                    report.AppendLine("</style></head><body>");

                    report.AppendLine("<h1>Compression Analysis Report</h1>");
                    report.AppendLine($"<p>Generated: {DateTime.Now}</p>");
                    report.AppendLine($"<p>Input File Size: {_originalText.Length} bytes</p>");

                    // Add Original Text Section
                    report.AppendLine("<h2>Original Text</h2>");
                    report.AppendLine("<div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 10px 0;'>");
                    report.AppendLine("<pre style='white-space: pre-wrap; word-wrap: break-word;'>");
                    report.AppendLine(System.Web.HttpUtility.HtmlEncode(_originalText));
                    report.AppendLine("</pre>");
                    report.AppendLine("</div>");

                    // Add comparison table
                    report.AppendLine("<h2>Comparison Table</h2>");
                    report.AppendLine("<table>");
                    report.AppendLine("<tr><th>Metric</th><th>Huffman Coding</th><th>Run-Length Coding</th></tr>");
                    report.AppendLine($"<tr><td>Compressed Size (bytes)</td><td>{_huffmanMetrics.CompressedSize}</td><td>{_runLengthMetrics.CompressedSize}</td></tr>");
                    report.AppendLine($"<tr><td>Compression Ratio</td><td>{_huffmanMetrics.CompressionRatio:F4}</td><td>{_runLengthMetrics.CompressionRatio:F4}</td></tr>");
                    report.AppendLine($"<tr><td>Entropy Before</td><td>{_huffmanMetrics.EntropyBefore:F4}</td><td>{_runLengthMetrics.EntropyBefore:F4}</td></tr>");
                    report.AppendLine($"<tr><td>Entropy After</td><td>{_huffmanMetrics.EntropyAfter:F4}</td><td>{_runLengthMetrics.EntropyAfter:F4}</td></tr>");
                    report.AppendLine($"<tr><td>Average Word Length</td><td>{_huffmanMetrics.AverageWordLength:F4}</td><td>{_runLengthMetrics.AverageWordLength:F4}</td></tr>");
                    report.AppendLine($"<tr><td>Computation Time (ms)</td><td>{_huffmanMetrics.ComputationTime}</td><td>{_runLengthMetrics.ComputationTime}</td></tr>");
                    report.AppendLine("</table>");

                    // Add analysis section
                    report.AppendLine("<h2>Analysis</h2>");
                    report.AppendLine("<p>Performance Analysis:</p>");
                    report.AppendLine("<ul>");
                    if (_huffmanMetrics.CompressedSize < _runLengthMetrics.CompressedSize)
                    {
                        report.AppendLine("<li>Huffman Coding achieved better compression ratio</li>");
                    }
                    else
                    {
                        report.AppendLine("<li>Run-Length Coding achieved better compression ratio</li>");
                    }

                    if (_huffmanMetrics.ComputationTime < _runLengthMetrics.ComputationTime)
                    {
                        report.AppendLine("<li>Huffman Coding was faster</li>");
                    }
                    else
                    {
                        report.AppendLine("<li>Run-Length Coding was faster</li>");
                    }
                    report.AppendLine("</ul>");

                    report.AppendLine("</body></html>");

                    File.WriteAllText(saveFileDialog.FileName, report.ToString());
                    StatusText.Text = $"Detailed report saved to: {saveFileDialog.FileName}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving detailed report: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            InputText.Clear();
            FileSizeRangeText.Text = "File Size Range: Not Analyzed";
            IntegerRangeText.Text = "Integer Range: Not Analyzed";
            VerifyOriginalText.Clear();
            VerifyDecompressedText.Clear();
            HuffmanResultsGrid.ItemsSource = null;
            RunLengthResultsGrid.ItemsSource = null;
            MetricsText.Text = string.Empty;
            CompressionProgress.Value = 0;
            StatusText.Text = "Ready";

            _originalText = null;
            _huffmanCompressed = null;
            _runLengthCompressed = null;
            _huffmanMetrics = null;
            _runLengthMetrics = null;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private double CalculateEntropy(string text)
        {
            var frequencies = text.GroupBy(c => c)
                                .ToDictionary(g => g.Key, g => g.Count());

            double entropy = 0;
            int totalChars = text.Length;

            foreach (var freq in frequencies.Values)
            {
                double probability = (double)freq / totalChars;
                entropy -= probability * Math.Log(probability, 2);
            }

            return entropy;
        }
    }

    public class CompressionMetrics
    {
        public int OriginalSize { get; set; }
        public int CompressedSize { get; set; }
        public double CompressionRatio { get; set; }
        public double EntropyBefore { get; set; }
        public double EntropyAfter { get; set; }
        public double AverageWordLength { get; set; }
        public long ComputationTime { get; set; }
    }

    public class ResultItem
    {
        public string Metric { get; set; }
        public object Value { get; set; }

        public ResultItem(string metric, object value)
        {
            Metric = metric;
            Value = value;
        }
    }
}