using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yadd.core
{
    public class DeltaScript
    {
        public string Name { get; internal set; }
        public string Code { get; internal set; }
    }

    public class Delta
    {
        public DeltaId Id { get; internal set; }
        public string CommitMessage { get; internal set; }
        public DeltaScript[] Scripts { get; internal set; }
        public BaselineId ParentBaselineId { get; internal set; }
    }
}
