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
    }
}
