using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;

namespace yadd.core
{
    public class Job
    {
        public string Name { get; private set; }
        public string TextualRepresentation { get; private set; }
        

        public Job(string pathToScript)
        {
            Name = Path.GetFileNameWithoutExtension(pathToScript);
            TextualRepresentation = File.ReadAllText(pathToScript);
        }

        public IEnumerable<JobStep> GetSteps()
        {
            throw new NotImplementedException();
        }
    }
}