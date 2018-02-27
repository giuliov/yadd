using System;
using System.Data.Common;

namespace yadd.core
{
    public class JobStep
    {
        public JobStep(Job parent, int num, string command)
        {
            Parent = parent;
            Number = num;
            this.Command = command;
        }

        public Job Parent { get; private set; }
        public int Number { get; private set; }
        public string Command { get; private set; }
    }
}