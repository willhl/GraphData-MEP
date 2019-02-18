using System.Collections.Generic;
using Neo4j.Driver.V1;


namespace HLApps.MEPGraph
{
    delegate void OnCommit(IStatementResult result);

    class PendingCypher
    {
        public string Query { get; set; }
        public Dictionary<string, object> Props { get; set; }

        public OnCommit Committed { get; set; }
    }
}
