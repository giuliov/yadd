using System.IO;

namespace yadd.core
{
    public class Repository
    {
        public static string Init()
        {
            var repo = Directory.CreateDirectory(".yadd");
            var baselineDir = repo.CreateSubdirectory("baseline");
            return baselineDir.FullName;
        }

        public static string FindRepo()
        {
            string current = Directory.GetCurrentDirectory();
            while (!Directory.Exists(Path.Combine(current,".yadd")))
            {
                current = Directory.GetParent(current).FullName;
            }
            return Path.Combine(current, ".yadd");
        }
    }
}
