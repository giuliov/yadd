using System;
using System.Collections.Generic;
using System.Text;

namespace yadd.core
{
    public class ConsoleLogger : Logger
    {
        public override void Write(string message)
        {
            Console.WriteLine(message);
        }

        public override void WriteError(string message)
        {
            ConsoleColor save = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = save;
        }
    }
}
