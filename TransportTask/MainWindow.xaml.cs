using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;

namespace TransportTask
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TextBox[,] OrygMatrix;
        int[,] OporPl;
        public MainWindow()
        {
            InitializeComponent();
            Rows.SelectedIndex = 0;
            Columns.SelectedIndex = 0;      
        }

        public void CreateTable(int Row, int Col)
        {
            OrygMatrix = new TextBox[Row+1, Col+1];

            for (int i = 0; i < Col+1; i++)
            {
                ColumnDefinition column = new ColumnDefinition
                {
                    Width = new GridLength(1, GridUnitType.Star)
                };
                OryginalMatrix.ColumnDefinitions.Add(column);
            }

            for (int j = 0; j < Row + 1; j++)
            {
                RowDefinition row = new RowDefinition
                {
                    Height = new GridLength(1, GridUnitType.Star)
                };
                OryginalMatrix.RowDefinitions.Add(row);
            }

            for (int row = 0; row < Row + 1; row++)
            {
                for (int col = 0; col < Col + 1; col++)
                {
                    TextBox txtbox;
                    if (Row == row && Col == col) {
                        txtbox = new TextBox
                        {
                            IsReadOnly = true,
                            Background = new SolidColorBrush(Colors.Yellow),
                            Text = $"Пот/Пр",
                            Name = $"a{row}{col}",
                        };
                        Grid.SetRow(txtbox, row);
                        Grid.SetColumn(txtbox, col);
                        OryginalMatrix.Children.Add(txtbox);
                    }

                    else if (Col == col || Row == row) 
                    {
                        txtbox = new TextBox
                        {
                            Background = new SolidColorBrush(Colors.Yellow),
                            Name = $"a{row}{col}",
                        };
                        Grid.SetRow(txtbox, row);
                        Grid.SetColumn(txtbox, col);
                        OryginalMatrix.Children.Add(txtbox);
                    }

                    else
                    {
                        txtbox = new TextBox
                        {
                            Name = $"a{row}{col}",
                        };
                        Grid.SetRow(txtbox, row);
                        Grid.SetColumn(txtbox, col);
                        OryginalMatrix.Children.Add(txtbox);
                    }

                    OrygMatrix[row, col] = txtbox;
                }
            }
        }

        private void Rows_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OryginalMatrix.RowDefinitions.Clear();
            OryginalMatrix.ColumnDefinitions.Clear();
            OryginalMatrix.Children.Clear();
            CreateTable(Rows.SelectedIndex + 1, Columns.SelectedIndex + 1);
        }

        private void Columns_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OryginalMatrix.RowDefinitions.Clear();
            OryginalMatrix.ColumnDefinitions.Clear();
            OryginalMatrix.Children.Clear();
            CreateTable(Rows.SelectedIndex+1,Columns.SelectedIndex+1);
        }

        public void GetOporP(int p, bool isWritingFile)
        {
            int rows = Rows.SelectedIndex + 1;
            int cols = Columns.SelectedIndex + 1;

            int[,] matrix = new int[rows+1, cols+1];

            for (int i = 0; i < OrygMatrix.GetLength(0)-1; i++)
            {
                for (int j = 0; j < OrygMatrix.GetLength(1)-1; j++)
                {
                    try
                    {
                        matrix[i, j] = int.Parse(OrygMatrix[i, j].Text);
                    }
                    catch (Exception)
                    {
                        matrix[i, j] = 0;
                    }   
                }
            }

            int[] supply = new int[rows];
            for (int i = 0; i < rows; i++)
            {
                    if (OrygMatrix[i, OrygMatrix.GetLength(1)-1] is TextBox textBox && Grid.GetRow(textBox) == i && Grid.GetColumn(textBox) == cols)
                    {
                        try
                        {
                            supply[i] = int.Parse(textBox.Text);
                        }
                        catch (Exception)
                        {
                            supply[i] = 0;
                        }
                        
                    }
            }

            int[] demand = new int[cols];
            for (int j = 0; j < cols; j++)
            {
                    if (OrygMatrix[OrygMatrix.GetLength(0) - 1, j] is TextBox textBox && Grid.GetRow(textBox) == rows && Grid.GetColumn(textBox) == j)
                    {
                        try
                        {
                            demand[j] = int.Parse(textBox.Text);
                        }
                        catch (Exception)
                        {
                            demand[j] = 0;
                        }
                       
                    }
            }

            try
            {
                int totalSupply = 0;
                int totalDemand = 0;

                for (int i = 0; i < rows; i++) totalSupply += supply[i];
                for (int j = 0; j < cols; j++) totalDemand += demand[j];

                if (totalSupply != totalDemand)
                {
                    if (totalSupply > totalDemand)
                    {
                        int[] newDemand = new int[cols + 1];
                        Array.Copy(demand, newDemand, cols);
                        newDemand[cols] = totalSupply - totalDemand;
                        demand = newDemand;

                        int[,] newMatrix = new int[rows, cols + 1];
                        for (int i = 0; i < rows; i++)
                            for (int j = 0; j < cols; j++)
                                newMatrix[i, j] = matrix[i, j];
                        matrix = newMatrix;

                        cols++;
                    }
                    else
                    {
                        int[] newSupply = new int[rows + 1];
                        Array.Copy(supply, newSupply, rows);
                        newSupply[rows] = totalDemand - totalSupply;
                        supply = newSupply;

                        int[,] newMatrix = new int[rows + 1, cols];
                        for (int i = 0; i < rows; i++)
                            for (int j = 0; j < cols; j++)
                                newMatrix[i, j] = matrix[i, j];
                        matrix = newMatrix;

                        rows++;
                    }
                }

                int[] trueSupply = new int[supply.Length];
                int[] trueDemand = new int[demand.Length];
                for (int i = 0; i < trueSupply.Length; i++) { trueSupply[i] = supply[i]; }
                for (int i = 0; i < trueDemand.Length; i++) { trueDemand[i] = demand[i]; }
                int[,] result = new int[rows, cols];
                OporPl = new int[rows, cols];

                if (p == 0)
                {
                    CalculateUsingNorthWestMethod(matrix, supply, demand, result);
                }
                else
                {
                    CalculateUsingMinimumElementMethod(matrix, supply, demand, result);
                }

                int totalCost = CalculateTotalCost(result, matrix);

                if (isWritingFile)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Опорный план:");
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < cols; j++)
                        {
                            sb.Append(result[i, j].ToString().PadRight(5));
                        }
                        sb.AppendLine();
                    }
                    sb.AppendLine($"Стоимость: {totalCost} у.е.");
                    File.WriteAllText("transport_solution.txt", sb.ToString());
                    MessageBox.Show("Ответ записан в файл transport_solution.txt");
                }

                PopulateGridWithResults(rows, cols, trueSupply, trueDemand, result, totalCost);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        private void CalculateUsingNorthWestMethod(int[,] matrix, int[] supply, int[] demand, int[,] result)
        {
            int row = 0, col = 0;
            while (row < supply.Length && col < demand.Length)
            {
                int allocation = Math.Min(supply[row], demand[col]);
                result[row, col] = allocation;
                OporPl[row, col] = allocation;
                supply[row] -= allocation;
                demand[col] -= allocation;

                if (supply[row] == 0) row++;
                else if (demand[col] == 0) col++;
            }
        }

        private void CalculateUsingMinimumElementMethod(int[,] matrix, int[] supply, int[] demand, int[,] result)
        {
            int[,] newMatrix = new int[Rows.SelectedIndex + 2, Columns.SelectedIndex + 1];
            for (int row = 0; row < Rows.SelectedIndex + 1; row++)
            {
                for (int col = 0; col < Columns.SelectedIndex + 1; col++)
                {
                    newMatrix[row, col] = matrix[row, col];
                }
            }

            while (true)
            {
                int minCost = int.MaxValue;
                int minRow = -1, minCol = -1;

                for (int i = 0; i < supply.Length; i++)
                {
                    for (int j = 0; j < demand.Length; j++)
                    {
                        if (newMatrix[i, j] < minCost && supply[i] > 0 && demand[j] > 0)
                        {
                            minCost = newMatrix[i, j];
                            minRow = i;
                            minCol = j;
                        }
                    }
                }
                if (minRow == -1 || minCol == -1) break;

                int allocation = Math.Min(supply[minRow], demand[minCol]);
                result[minRow, minCol] = allocation;
                OporPl[minRow, minCol] = allocation;

                supply[minRow] -= allocation;
                demand[minCol] -= allocation;

                if (supply[minRow] == 0)
                {
                    for (int i = 0; i < demand.Length; i++)
                        newMatrix[minRow, i] = int.MaxValue;
                }

                if (demand[minCol] == 0)
                {
                    for (int i = 0; i < supply.Length; i++)
                        newMatrix[i, minCol] = int.MaxValue;
                }
            }
        }

        private int CalculateTotalCost(int[,] result, int[,] matrix)
        {
            int totalCost = 0;
            for (int i = 0; i < result.GetLength(0); i++)
            {
                for (int j = 0; j < result.GetLength(1); j++)
                {
                    totalCost += result[i, j] * matrix[i, j];
                }
            }
            return totalCost;
        }

        private void PopulateGridWithResults(int rows, int cols, int[] supply, int[] demand, int[,] result, int totalCost)
        {
            OpornPlan.RowDefinitions.Clear();
            OpornPlan.ColumnDefinitions.Clear();
            OpornPlan.Children.Clear();

            for (int i = 0; i < cols + 1; i++)
            {
                ColumnDefinition column = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
                OpornPlan.ColumnDefinitions.Add(column);
            }

            for (int i = 0; i < rows + 1; i++)
            {
                RowDefinition rowDef = new RowDefinition { Height = new GridLength(1, GridUnitType.Star) };
                OpornPlan.RowDefinitions.Add(rowDef);
            }

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    TextBox txtBox = new TextBox
                    {
                        Text = result[i, j].ToString(),
                        IsReadOnly = true,
                    };
                    Grid.SetRow(txtBox, i);
                    Grid.SetColumn(txtBox, j);
                    OpornPlan.Children.Add(txtBox);
                }

                TextBox txtSupply = new TextBox
                {
                    Text = supply[i].ToString(),
                    IsReadOnly = true,
                    Background = new SolidColorBrush(Colors.Yellow)
                };
                Grid.SetRow(txtSupply, i);
                Grid.SetColumn(txtSupply, cols);
                OpornPlan.Children.Add(txtSupply);
            }

            for (int j = 0; j < cols; j++)
            {
                TextBox txtDemand = new TextBox
                {
                    Text = demand[j].ToString(),
                    IsReadOnly = true,
                    Background = new SolidColorBrush(Colors.Yellow)
                };
                Grid.SetRow(txtDemand, rows);
                Grid.SetColumn(txtDemand, j);
                OpornPlan.Children.Add(txtDemand);
            }

            TextBox txtCost = new TextBox
            {
                Text = "Стоимость\n" + totalCost.ToString() + " у.е.",
                IsReadOnly = true,
                Background = new SolidColorBrush(Colors.LightBlue)
            };
            Grid.SetRow(txtCost, rows);
            Grid.SetColumn(txtCost, cols);
            OpornPlan.Children.Add(txtCost);
        }

        private void OporP_Click(object sender, RoutedEventArgs e)
        {
            int Met = Method.SelectedIndex;
            GetOporP(Met, false);
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            OryginalMatrix.RowDefinitions.Clear();
            OryginalMatrix.ColumnDefinitions.Clear();
            OryginalMatrix.Children.Clear();

            OpornPlan.RowDefinitions.Clear();
            OpornPlan.ColumnDefinitions.Clear();
            OpornPlan.Children.Clear();

            CreateTable(Rows.SelectedIndex + 1, Columns.SelectedIndex + 1);
        }

        private void LoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Text documents (*.txt)|*.txt|All files (*.*)|*.*";
            dialog.FilterIndex = 2;

            Nullable<bool> result = dialog.ShowDialog();

            if (result == true)
            {
                // Open document
                string filename = dialog.FileName;
                string[] fileText = System.IO.File.ReadAllLines(filename);

                //try
                //{

                    int[][] jagged = fileText.Select(x => x.Split(' ').Select(int.Parse).ToArray()).ToArray();

                    int rows = jagged.Length;

                    int columns = jagged[0].Length;

                Rows.SelectedIndex = rows - 2;
                Columns.SelectedIndex = columns - 2;

                    int[,] constraints = new int[rows, columns];

                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < columns; j++)
                        {

                            if (i == rows - 1 && j == columns - 1) continue;
                            constraints[i, j] = jagged[i][j];
                        }
                    }

                    CreateTable(constraints);



                //}
                //catch (Exception ex)
                //{
                //    MessageBox.Show(ex.ToString());
                //}
            }
        }

        private void CreateTable(int[,] elements)
        {
            int rows = elements.GetUpperBound(0) + 1;

            int columns = elements.Length / rows;

            OrygMatrix = new TextBox[rows, columns];

            for (int i = 0; i < columns + 1; i++)
            {
                ColumnDefinition column = new ColumnDefinition
                {
                    Width = new GridLength(1, GridUnitType.Star)
                };
                OryginalMatrix.ColumnDefinitions.Add(column);
            }

            for (int j = 0; j < columns + 1; j++)
            {
                RowDefinition row = new RowDefinition
                {
                    Height = new GridLength(1, GridUnitType.Star)
                };
                OryginalMatrix.RowDefinitions.Add(row);
            }

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    TextBox txtbox;
                    if (rows == row && columns == col)
                    {
                        txtbox = new TextBox
                        {
                            IsReadOnly = true,
                            Background = new SolidColorBrush(Colors.Yellow),
                            Text = $"Пот/Пр",
                            Name = $"a{row}{col}",
                        };
                        Grid.SetRow(txtbox, row);
                        Grid.SetColumn(txtbox, col);
                        OryginalMatrix.Children.Add(txtbox);
                    }

                    else if (columns == col || rows == row)
                    {
                        txtbox = new TextBox
                        {
                            Background = new SolidColorBrush(Colors.Yellow),
                            Name = $"a{row}{col}",
                        };
                        Grid.SetRow(txtbox, row);
                        Grid.SetColumn(txtbox, col);
                        OryginalMatrix.Children.Add(txtbox);
                    }

                    else
                    {
                        txtbox = new TextBox
                        {
                            Name = $"a{row}{col}",
                            Text = elements[row, col].ToString(),
                        };
                        Grid.SetRow(txtbox, row);
                        Grid.SetColumn(txtbox, col);
                        OryginalMatrix.Children.Add(txtbox);
                    }

                    OrygMatrix[row, col] = txtbox;
                }
            }
        }

        private void GetAnswerAndWriteFile(object sender, RoutedEventArgs e)
        {
            int Met = Method.SelectedIndex;
            GetOporP(Met, true);
        }
    }
}
