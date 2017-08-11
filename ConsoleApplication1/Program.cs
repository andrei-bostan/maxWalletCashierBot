using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiCalls;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var results = Helper.GetCashiers();
            Console.WriteLine(results.FirstOrDefault());
            Console.ReadLine();
        }
    }
}
