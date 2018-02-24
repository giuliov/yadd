using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;

namespace yadd.core
{
    public class Job
    {
        public string Name { get; private set; }
        public IEnumerable<JobStep> Steps { get; set; }

        public Job(string pathToScript)
        {
            Name = Path.GetFileNameWithoutExtension(pathToScript);
            string script = File.ReadAllText(pathToScript);
            Steps = Parse(script);
        }

        public string GetTextualRepresentation()
        {
        }

        private static IEnumerable<JobStep> Parse(string script)
        {
            throw new NotImplementedException();
        }
    }
}