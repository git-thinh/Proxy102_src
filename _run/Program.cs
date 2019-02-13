using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp12
{
    class Program
    {
        static void Main(string[] args)
        {
            //System.Diagnostics.Process.Start(@"%SystemRoot%\system32\WindowsPowerShell\v1.0\powershell.exe");
            System.Diagnostics.Process.Start(@"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe", @"./Proxy102.exe");
            //Console.ReadLine();
        }
    }
}
