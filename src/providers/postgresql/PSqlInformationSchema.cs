using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using yadd.core;

namespace yadd.postgresql_provider
{
    internal class PSqlInformationSchema : InformationSchema
    {
        public PSqlInformationSchema(IList<InformationSchemata> schemata, IList<InformationSchemaTable> tables)
        {
            Schemata = schemata;
            Tables = tables;
        }
    }
}
