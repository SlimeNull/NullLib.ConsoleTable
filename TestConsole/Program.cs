using NullLib.ConsoleTable;
using System;
using System.Collections;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var table = new ConsoleTable("one", "two", "three");
            table.AddRow(1, 2, 3)
                 .AddRow("this line should be longer", "yes it is", "oh")
                 .AddRow("this line should be longer", "我们甚至还支持中文, 完全不会错乱!", "oh");

            table.SetColumnAlignment(2, ColumnAlignment.Right);

            Console.WriteLine("\n\nFormat: Default");
            Console.WriteLine(table.ToDefaultString());

            table.TableMinimiumWidth = 100;

            Console.WriteLine("\n\nFormat: Markdown");
            Console.WriteLine(table.ToMarkdownString());

            table.TableMinimiumWidth = 0;
            table.TableMaximiumWidth = 50;
            Console.WriteLine("\n\nFormat: Alternative");
            Console.WriteLine(table.ToAlternativeString());

            Console.WriteLine("\n\nFormat: Minimal");
            Console.WriteLine(table.ToMinimalString());

            Console.ReadLine();

        }
    }
}
