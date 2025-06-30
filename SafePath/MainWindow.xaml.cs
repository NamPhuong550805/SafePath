using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SafePathFinder
{
    public partial class MainWindow : Window
    {
        private int gridSize = 0;
        private Button[,] cells;
        private List<List<(int, int)>> allPaths = new List<List<(int, int)>>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void GenerateMap_Click(object sender, RoutedEventArgs e)
        {
            ErrorTextBlock.Text = "";
            PathCountTextBlock.Text = "";
            if (!int.TryParse(SizeTextBox.Text, out gridSize) || gridSize <= 0 || gridSize > 20)
            {
                ErrorTextBlock.Text = "Vui lòng nhập kích thước hợp lệ.";
                return;
            }

            MapGrid.Rows = gridSize;
            MapGrid.Columns = gridSize;
            MapGrid.Children.Clear();
            cells = new Button[gridSize, gridSize];

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    var btn = new Button
                    {
                        Tag = (i, j),
                        Background = Brushes.White,
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(0.5),
                        Width = 30,
                        Height = 30,
                        Margin = new Thickness(1),
                        ToolTip = $"({i},{j})"
                    };
                    btn.Click += Cell_Click;
                    cells[i, j] = btn;
                    MapGrid.Children.Add(btn);
                }
            }

            PathsListPanel.Items.Clear();
            allPaths.Clear();
        }

        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var (i, j) = ((int, int))btn.Tag;

            if (btn.Background == Brushes.White)
                btn.Background = Brushes.Black;
            else if (btn.Background == Brushes.Black)
                btn.Background = Brushes.White;
        }

        private void CalculatePaths_Click(object sender, RoutedEventArgs e)
        {
            if (cells == null || gridSize == 0)
            {
                ErrorTextBlock.Text = "Vui lòng tạo bản đồ trước.";
                return;
            }

            ErrorTextBlock.Text = "";
            PathCountTextBlock.Text = "";
            PathsListPanel.Items.Clear();
            allPaths.Clear();

            bool[,] grid = new bool[gridSize, gridSize];
            for (int i = 0; i < gridSize; i++)
                for (int j = 0; j < gridSize; j++)
                    grid[i, j] = cells[i, j].Background != Brushes.Black;

            if (!grid[0, 0] || !grid[gridSize - 1, gridSize - 1])
            {
                ErrorTextBlock.Text = "Điểm bắt đầu hoặc kết thúc bị chặn.";
                return;
            }

            // Tính số đường đi bằng DP
            int count = CountPathsDP(grid);
            PathCountTextBlock.Text = $"🔢 Số lượng đường đi an toàn: {count}";

            // Duyệt các đường đi bằng Backtracking
            List<(int, int)> path = new List<(int, int)>();
            DFS(grid, 0, 0, path);

            if (allPaths.Count == 0)
            {
                PathsListPanel.Items.Add("Không có đường đi an toàn.");
            }
            else
            {
                int index = 1;
                foreach (var p in allPaths)
                {
                    string display = $"#{index++}: " + string.Join(" -> ", p);
                    PathsListPanel.Items.Add(display);
                }
            }
        }

        private int CountPathsDP(bool[,] grid)
        {
            int[,] dp = new int[gridSize, gridSize];

            if (!grid[0, 0]) return 0;
            dp[0, 0] = 1;

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    if (!grid[i, j]) continue;
                    if (i > 0) dp[i, j] += dp[i - 1, j];
                    if (j > 0) dp[i, j] += dp[i, j - 1];
                }
            }

            return dp[gridSize - 1, gridSize - 1];
        }

        private void DFS(bool[,] grid, int x, int y, List<(int, int)> path)
        {
            if (x >= gridSize || y >= gridSize || !grid[x, y])
                return;

            path.Add((x, y));

            if (x == gridSize - 1 && y == gridSize - 1)
            {
                allPaths.Add(new List<(int, int)>(path));
            }
            else
            {
                DFS(grid, x + 1, y, path);
                DFS(grid, x, y + 1, path);
            }

            path.RemoveAt(path.Count - 1);
        }

        private void ViewPath_Click(object sender, RoutedEventArgs e)
        {
            string pathString = (string)((Button)sender).Tag;
            var coords = ParsePath(pathString);

            for (int i = 0; i < gridSize; i++)
                for (int j = 0; j < gridSize; j++)
                    if (cells[i, j].Background != Brushes.Black)
                        cells[i, j].Background = Brushes.White;

            foreach (var (i, j) in coords)
                if (cells[i, j].Background != Brushes.Black)
                    cells[i, j].Background = Brushes.LightGreen;
        }

        private List<(int, int)> ParsePath(string pathString)
        {
            var coords = new List<(int, int)>();
            var parts = pathString.Split(new[] { "->", "#", ":" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var trimmed = part.Trim().Trim('(', ')');
                if (trimmed.Contains(","))
                {
                    var nums = trimmed.Split(',');
                    if (int.TryParse(nums[0], out int x) && int.TryParse(nums[1], out int y))
                        coords.Add((x, y));
                }
            }

            return coords;
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (cells == null) return;

            for (int i = 0; i < gridSize; i++)
                for (int j = 0; j < gridSize; j++)
                    cells[i, j].Background = Brushes.White;

            PathsListPanel.Items.Clear();
            allPaths.Clear();
            ErrorTextBlock.Text = "";
            PathCountTextBlock.Text = "";
        }
    }
}
