using System;
using System.Runtime.ExceptionServices;

namespace PruebasVarias
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var today = DateTime.Today;
            Console.WriteLine(today);
            var month = new DateTime(today.Year, today.Month, 1);
            Console.WriteLine(month);
            var first = month.AddMonths(-1);
            Console.WriteLine(first);

            var first1 = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            Console.WriteLine(first1);

        }
    }
}
