using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Layout;
using System.Data;

namespace SimpleCalculator
{
    public partial class MainWindow : Window
    {
        private TextBlock display;

        public MainWindow()
        {
            Title = "ماشین حساب";
            Width = 300;
            Height = 400;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var grid = new Grid();
            for (int i = 0; i < 5; i++) grid.RowDefinitions.Add(new RowDefinition());
            for (int i = 0; i < 4; i++) grid.ColumnDefinitions.Add(new ColumnDefinition());

            display = new TextBlock { FontSize = 32, TextAlignment = TextAlignment.Right, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10) };
            Grid.SetRow(display, 0);
            Grid.SetColumnSpan(display, 4);
            grid.Children.Add(display);

            string[] buttons = {
                "7", "8", "9", "/",
                "4", "5", "6", "*",
                "1", "2", "3", "-",
                "C", "0", "=", "+"
            };

            for (int i = 0; i < buttons.Length; i++)
            {
                var btn = new Button { Content = buttons[i], FontSize = 20 };
                btn.Click += Button_Click;
                Grid.SetRow(btn, (i / 4) + 1);
                Grid.SetColumn(btn, i % 4);
                grid.Children.Add(btn);
            }

            Content = grid;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string value = (sender as Button).Content.ToString();

            if (value == "C")
            {
                display.Text = "";
            }
            else if (value == "=")
            {
                try
                {
                    var result = new DataTable().Compute(display.Text, null);
                    display.Text = result.ToString();
                }
                catch
                {
                    display.Text = "خطا";
                }
            }
            else
            {
                display.Text += value;
            }
        }

        [STAThread]
        public static void Main()
        {
            var app = new Application();
            app.Run(new MainWindow());
        }
    }
}