using System;
using System.Collections.Generic;
using System.Linq;

using Neo4j.Driver.V1;

namespace HLApps.MEPGraph
{


    public class Neo4jClient : IDisposable, IGraphDBClient
    {

        IDriver _driver;
        public Neo4jClient(Uri host, string userName, string password)
        {
            _driver = GraphDatabase.Driver(host, AuthTokens.Basic(userName, password));
        }

        HashSet<string> constrained = new HashSet<string>();
        Queue<PendingCypher> commitStack = new Queue<PendingCypher>();
        public void Commit()
        {
            using (var session = _driver.Session())
            {
                while (commitStack.Count > 0)
                {
                    var pendingQuery = commitStack.Dequeue();
                    var wtxResult = session.WriteTransaction(tx =>
                    {
                        var result = pendingQuery.Props != null && pendingQuery.Props.Count > 0 ? tx.Run(pendingQuery.Query, pendingQuery.Props) : tx.Run(pendingQuery.Query);
                        return result;
                    });

                    pendingQuery.Committed?.Invoke(wtxResult);
                }
            }

        }

        /// <summary>
        ///  MATCH(a),(b)
        ///  WHERE ID(a) = $fromNodeId AND ID(b) = $toNodeId
        ///  CREATE (a)-[r: $relType $variables ]->(b)
        /// </summary>
        /// <param name="fromNodeId"></param>
        /// <param name="toNodeId"></param>
        /// <param name="relType"></param>
        /// <param name="variables"></param>
        public void Relate(PendingNode fromNodeId, PendingNode toNodeId, Model.MEPEdgeTypes relType, Dictionary<string, object> variables)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("frid", fromNodeId.TempId);
            props.Add("toid", toNodeId.TempId);

            string query = string.Empty;
            if (variables != null && variables.Count > 0)
            {
                props.Add("cvar", variables);
                query =
                    string.Format("MATCH(a: {0} {{TempId: $frid}}),(b:{1} {{TempId: $toid}})", fromNodeId.Node.Label, toNodeId.Node.Label) +
                    string.Format("CREATE (a)-[r:{0} $cvar]->(b) ", relType);
            }
            else
            {
                query =
                    string.Format("MATCH(a: {0} {{TempId: $frid}}),(b:{1} {{TempId: $toid}})", fromNodeId.Node.Label, toNodeId.Node.Label) +
                    string.Format("CREATE (a)-[r:{0}]->(b) ", relType);
            }

            var pec = new PendingCypher();
            pec.Query = query;
            pec.Props = props;

            pec.Committed = (IStatementResult result) =>
             {
                 var rs = result;

             };

            commitStack.Enqueue(pec);
            /*using (var session = _driver.Session())
            {
                var wtxResult = session.WriteTransaction(tx =>
                {
                    var result = props.Count > 0 ? tx.Run(query, props) : tx.Run(query);
                    return result;
                });

                Console.WriteLine(wtxResult);
            }
            */

        }

       
        public void Relate(Model.Node fromNode, Model.Node toNode, string relType, Dictionary<string, object> variables)
        {
          
        }


        
       
        public PendingNode Push(Model.Node node, Dictionary<string, object> variables)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("props", variables);
           
            

            var pendingNode = new PendingNode(node);
            if (variables.ContainsKey(pendingNode.TempId))
            {
                variables.Add("TempId", pendingNode.TempId);
            }
            else
            {
                variables["TempId"] = pendingNode.TempId;
            }

            var nodeLabel = node.Label;
            var query = string.Format("CREATE (n:{0} $props)", nodeLabel);


            if (!constrained.Contains(nodeLabel))
            {
                var pecCs = new PendingCypher();
                pecCs.Query = string.Format("CREATE CONSTRAINT ON(n:{0}) ASSERT n.TempId IS UNIQUE", nodeLabel);
                commitStack.Enqueue(pecCs); 
            }

            var pec = new PendingCypher();
            pec.Query = query;
            pec.Props = props;
            commitStack.Enqueue(pec);

            /*
            using (var session = _driver.Session())
            {
                session.WriteTransaction(tx =>
                {
                    //ensure id is constrained to improve performance when relating nodes
                    if (!constrained.Contains(nodeLabel))
                    {
                        tx.Run(string.Format("CREATE CONSTRAINT ON(n:{0}) ASSERT n.TempId IS UNIQUE", nodeLabel));
                        constrained.Add(nodeLabel);
                    }
                });

                retId = session.WriteTransaction(tx =>
                {
                    var result = props.Count > 0 ? tx.Run(query, props) : tx.Run(query);
                    
                    return result;
                });

            }

            if (retId != null)
            {
                var lr = retId.ToList();
                if (lr.Count() == 1)
                {
                    var rs = lr.First();
                    if (rs.Values.Count() == 1)
                    {
                        var rtval = rs.Values.First().Value;
                        pendingNode.NodeId = (long)rtval;
                    }
                }
                pendingNode.WasCommited = true;
            }
            */
            return pendingNode;

        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_driver != null) _driver.Dispose();
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion

    }
}
