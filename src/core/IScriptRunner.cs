using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yadd.core
{
    public interface IScriptRunner
    {
        (int err, string msg) Run(string scriptCode);
    }
}
