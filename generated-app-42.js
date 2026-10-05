using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Windows.Media;

namespace SimpleCalculator
{
    public class App : Application
    {
        [STAThread]
        public static void Main()
        {
            App app = new App();
            app.Run(new MainWindow());
        }
    }

    public class MainWindow : Window
    {
        private TextBox _display;
        private string _currentExpression = "";

        public MainWindow()
        {
            Title = "ماشین حساب";
            Width = 300;
            Height = 400;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;

            Grid grid = new Grid();
            for (int i = 0; i < 4; i++) grid.ColumnDefinitions.Add(new ColumnDefinition());
            for (int i = 0; i < 5; i++) grid.RowDefinitions.Add(new RowDefinition());

            _display = new TextBox 
            { 
                FontSize = 24, 
                IsReadOnly = true, 
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(5)
            };
            Grid.SetColumnSpan(_display, 4);
            grid.Children.Add(_display);

            string[,] buttons = {
                {"7", "8", "9", "/"},
                {"4", "5", "6", "*"},
                {"1", "2", "3", "-"},
                {"C", "0", "=", "+"}
            };

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Button btn = new Button { Content = buttons[i, j], FontSize = 18, Margin = new Thickness(2) };
                    btn.Click += Button_Click;
                    Grid.SetRow(btn, i + 1);
                    Grid.SetColumn(btn, j);
                    grid.Children.Add(btn);
                }
            }

            Content = grid;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string val = (sender as Button).Content.ToString();

            if (val == "C")
            {
                _currentExpression = "";
            }
            else if (val == "=")
            {
                try
                {
                    DataTable dt = new DataTable();
                    var result = dt.Compute(_currentExpression, "");
                    _currentExpression = result.ToString();
                }
                catch
                {
                    _currentExpression = "خطا";
                }
            }
            else
            {
                _currentExpression += val;
            }

            _display.Text = _currentExpression;
        }
    }
}