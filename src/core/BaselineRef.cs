using System;
using System.Text.RegularExpressions;

namespace yadd.core
{
    public class BaselineRef
    {
        static readonly Regex validateRef = new Regex(@"^[a-zA-Z0-9_\-\.]{0,64}$", RegexOptions.Compiled);
        protected string userInput;

        public BaselineRef(string s)
        {
            if (!validateRef.IsMatch(s)) throw new Exception("Invalid reference name");
            userInput = s;
        }

        public bool IsNull => string.IsNullOrEmpty(userInput);

        public string DirectoryMatchingPattern => userInput + "*";

        public override string ToString()
        {
            return userInput.ToLowerInvariant();
        }
    }
}
