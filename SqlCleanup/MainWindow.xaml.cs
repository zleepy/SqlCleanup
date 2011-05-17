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

            var testQuery1 = @"SELECT COUNT(1) AS Col1, Table1.Column1 AS Col2";
            var testQuery2 = @"SELECT Table1.Column1, Table2.Column2
FROM Table1 INNER JOIN Table2 T2 ON T2.Id = Table1.Id
WHERE T2.Id < 300 AND Table1.Id > 10 AND Table1.Name LIKE '%Svensson' 
ORDER BY T2.Id";

            var test3 = "server.[Databas 1].dbo.[Id Column 2]";

            //var p = new SqlTokenizer(x => new Lexer(x));
            //var result = p.Process(testQuery2);

            var parser = new sql1();
            var result = parser.Parse("SELECT COUNT(1) AS Col1, COUNT(1) Col3, Table1.Column1 AS Col2, Table1.Column1 FROM Table1 T2");



            Console.WriteLine("Antal = " + result.ToString());
        }
    }
}
