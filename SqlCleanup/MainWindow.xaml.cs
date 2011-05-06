using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SqlCleanup.Parser;

namespace SqlCleanup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var testQuery1 = @"SELECT * FROM Test WHERE (Col = 1)";
            var testQuery2 = @"SELECT Table1.Column1, Table2.Column2
FROM Table1 INNER JOIN Table2 T2 ON T2.Id = Table1.Id
WHERE T2.Id < 300 AND Table1.Id > 10 AND Table1.Name LIKE '%Svensson' 
ORDER BY T2.Id";

            var p = new SqlTokenizer(x => new Lexer(x));
            var result = p.Process(testQuery2);

            Console.WriteLine("Antal = " + result.Length);
        }
    }
}
