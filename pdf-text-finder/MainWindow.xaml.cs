using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using UglyToad.PdfPig;
using ClosedXML.Excel;

namespace PdfTextFinder
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<SearchResult> Results { get; set; } = new();
        public ObservableCollection<string> SearchHistory { get; set; } = new();
        private readonly string _historyFilePath;
        private CancellationTokenSource? _cts;

        public MainWindow()
        {
            InitializeComponent();
            dgResults.ItemsSource = Results;
            cmbSearchTerm.ItemsSource = SearchHistory;

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(appData, "PdfTextFinder");
            Directory.CreateDirectory(appFolder);
            _historyFilePath = Path.Combine(appFolder, "history.json");

            LoadHistory();
        }

        private void LoadHistory()
        {
            try
            {
                if (File.Exists(_historyFilePath))
                {
                    string json = File.ReadAllText(_historyFilePath);
                    var list = JsonSerializer.Deserialize<List<string>>(json);
                    if (list != null)
                    {
                        foreach (var item in list) SearchHistory.Add(item);
                    }
                }
            }
            catch { /* Ignore load errors */ }
        }

        private void SaveHistory()
        {
            try
            {
                string json = JsonSerializer.Serialize(SearchHistory.ToList());
                File.WriteAllText(_historyFilePath, json);
            }
            catch { /* Ignore save errors */ }
        }

        private void AddToHistory(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return;
            
            if (SearchHistory.Contains(term))
                SearchHistory.Remove(term);

            SearchHistory.Insert(0, term);

            while (SearchHistory.Count > 20)
                SearchHistory.RemoveAt(SearchHistory.Count - 1);

            SaveHistory();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select Folder to Search PDFs",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (dialog.ShowDialog() == true)
            {
                txtFolderPath.Text = dialog.FolderName;
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = txtFolderPath.Text;
            string searchTerm = cmbSearchTerm.Text;

            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                MessageBox.Show("Please select a valid folder path.", "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                MessageBox.Show("Please enter a search term.", "Empty Search", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AddToHistory(searchTerm);
            
            _cts = new CancellationTokenSource();
            await PerformSearch(folderPath, searchTerm, _cts.Token);
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
            btnStop.IsEnabled = false;
            lblStatus.Text = "Stopping search...";
        }

        private async Task PerformSearch(string folderPath, string searchTerm, CancellationToken token)
        {
            Results.Clear();
            btnSearch.IsEnabled = false;
            btnStop.IsEnabled = true;
            progressBar.Visibility = Visibility.Visible;
            progressBar.IsIndeterminate = true;
            lblStatus.Text = "Scanning for PDF files...";

            try
            {
                var files = await Task.Run(() => Directory.GetFiles(folderPath, "*.pdf", SearchOption.AllDirectories), token);
                
                if (files.Length == 0)
                {
                    lblStatus.Text = "No PDF files found.";
                    return;
                }

                progressBar.IsIndeterminate = false;
                progressBar.Maximum = files.Length;
                progressBar.Value = 0;

                int processedCount = 0;
                int matchCount = 0;

                foreach (var file in files)
                {
                    if (token.IsCancellationRequested)
                    {
                        lblStatus.Text = $"Search cancelled. Found matches in {matchCount} files.";
                        return;
                    }

                    processedCount++;
                    lblStatus.Text = $"Searching file {processedCount} of {files.Length}: {Path.GetFileName(file)}";
                    progressBar.Value = processedCount;

                    var result = await Task.Run(() => SearchInPdf(file, searchTerm, token), token);
                    if (result != null)
                    {
                        Results.Add(result);
                        matchCount++;
                    }
                }

                lblStatus.Text = $"Search complete. Found matches in {matchCount} files.";
            }
            catch (OperationCanceledException)
            {
                lblStatus.Text = "Search was cancelled by user.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Search failed.";
            }
            finally
            {
                btnSearch.IsEnabled = true;
                btnStop.IsEnabled = false;
                progressBar.Visibility = Visibility.Collapsed;
                _cts?.Dispose();
                _cts = null;
            }
        }

        private SearchResult? SearchInPdf(string filePath, string searchTerm, CancellationToken token)
        {
            try
            {
                using (var pdf = PdfDocument.Open(filePath))
                {
                    var matchingPages = new List<int>();
                    string firstMatchContext = string.Empty;

                    foreach (var page in pdf.GetPages())
                    {
                        if (token.IsCancellationRequested) return null;

                        string pageText = page.Text;
                        if (pageText.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                        {
                            matchingPages.Add(page.Number);

                            if (string.IsNullOrEmpty(firstMatchContext))
                            {
                                var lines = pageText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                var matchingLine = lines.FirstOrDefault(l => l.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                                if (matchingLine != null)
                                {
                                    firstMatchContext = matchingLine.Trim();
                                    if (firstMatchContext.Length > 200) 
                                        firstMatchContext = firstMatchContext.Substring(0, 197) + "...";
                                }
                            }
                        }
                    }

                    if (matchingPages.Count > 0)
                    {
                        return new SearchResult
                        {
                            FileName = Path.GetFileName(filePath),
                            FullPath = filePath,
                            PageNumbers = string.Join(", ", matchingPages),
                            Context = firstMatchContext
                        };
                    }
                }
            }
            catch
            {
                // Skip files that cannot be opened
            }
            return null;
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (Results.Count == 0)
            {
                MessageBox.Show("No results to export.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = "Search_Results.xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Results");
                        
                        worksheet.Cell(1, 1).Value = "File Name";
                        worksheet.Cell(1, 2).Value = "Pages";
                        worksheet.Cell(1, 3).Value = "Context";
                        worksheet.Cell(1, 4).Value = "Full Path";

                        var headerRange = worksheet.Range(1, 1, 1, 4);
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                        for (int i = 0; i < Results.Count; i++)
                        {
                            var item = Results[i];
                            worksheet.Cell(i + 2, 1).Value = item.FileName;
                            worksheet.Cell(i + 2, 2).Value = item.PageNumbers;
                            worksheet.Cell(i + 2, 3).Value = item.Context;
                            worksheet.Cell(i + 2, 4).Value = item.FullPath;
                        }

                        worksheet.Columns().AdjustToContents();
                        workbook.SaveAs(saveDialog.FileName);
                    }

                    MessageBox.Show("Results exported successfully!", "Export Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to export: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (dgResults.SelectedItem is SearchResult selected)
            {
                OpenFile(selected.FullPath);
            }
        }

        private void dgResults_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgResults.SelectedItem is SearchResult selected)
            {
                OpenFile(selected.FullPath);
            }
        }

        private void OpenFile(string path)
        {
            try
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmbSearchTerm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchButton_Click(sender, e);
            }
        }
    }
}