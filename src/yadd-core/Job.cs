using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace yadd.core
{
    public class Job
    {
        public string Name { get; private set; }
        public string TextualRepresentation { get; private set; }
        

        public Job(string pathToScript)
        {
            if (!File.Exists(pathToScript))
            {
                throw new ArgumentException($"Job requires a valid path to a script");
            }
            Name = Path.GetFileNameWithoutExtension(pathToScript);
            TextualRepresentation = File.ReadAllText(pathToScript);
        }

        public IEnumerable<JobStep> GetSteps()
        {
            // TODO breaking at GO is a MSSQL thing????
            IEnumerable<string> commandStrings = Regex.Split(TextualRepresentation, @"^\s*GO\s*$",
                         RegexOptions.Multiline | RegexOptions.IgnoreCase);
            int num = 1;
            foreach (var command in commandStrings)
            {
                yield return new JobStep(this, num++, command);
            }
        }

        public HashValue GetHash()
        {
            return new HashValue(TextualRepresentation);
        }
    }
}