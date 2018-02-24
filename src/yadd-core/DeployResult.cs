using System;
using System.Security.Cryptography.X509Certificates;

namespace yadd.core
{
    public class DeployResult : IComparable<DeployResult>
    {
        public int Errors { get; }
        public int Warnings { get; }

        public DeployResult(int errors, int warnings)
        {
            Errors = errors;
            Warnings = warnings;
        }

        public int CompareTo(DeployResult other)
        {
            return Errors == other.Errors
                ? Warnings.CompareTo(Warnings)
                : Errors.CompareTo(other.Errors);
        }
    }
}