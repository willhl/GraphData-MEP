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

        /// <summary>
        ///  MATCH(a),(b)
        ///  WHERE ID(a) = $fromNodeId AND ID(b) = $toNodeId
        ///  CREATE (a)-[r: $relType $variables ]->(b)
        /// </summary>
        /// <param name="fromNodeId"></param>
        /// <param name="toNodeId"></param>
        /// <param name="relType"></param>
        /// <param name="variables"></param>
        public void Relate(long fromNodeId, long toNodeId, Model.MEPEdgeTypes relType, Dictionary<string, object> variables)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("auid", fromNodeId);
            props.Add("buid", toNodeId);

            string query = string.Empty;
            if (variables != null && variables.Count > 0)
            {
                props.Add("cvar", variables);
                query =
                    "MATCH(a),(b)" +
                    "WHERE ID(a) = $auid AND ID(b) = $buid " +
                    string.Format("CREATE (a)-[r: {0} $cvar]->(b) ", relType);
            }
            else
            {
                query =
                    "MATCH(a),(b)" +
                    "WHERE ID(a) = $auid AND ID(b) = $buid " +
                    string.Format("CREATE (a)-[r: {0}]->(b) ", relType);
            }


            using (var session = _driver.Session())
            {
                var greeting = session.WriteTransaction(tx =>
                {
                    var result = props.Count > 0 ? tx.Run(query, props) : tx.Run(query);
                    return result;
                });

                Console.WriteLine(greeting);
            }


        }


        public void Relate(Model.Node fromNode, Model.Node toNode, string relType, Dictionary<string, object> variables)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("auid", fromNode.UniqueId);
            props.Add("buid", toNode.UniqueId);

            string query = string.Empty;
            if (variables != null && variables.Count > 0)
            {
                props.Add("cvar", variables);
                query =
                    string.Format("MATCH(a:{0}),(b:{1})", fromNode.Label, toNode.Label) +
                    "WHERE a.UniqueId = $auid AND b.UniqueId = $buid " +
                    string.Format("CREATE (a)-[r: {0} $cvar]->(b) ", relType);
            }
            else
            {
                query =
                    string.Format("MATCH(a:{0}),(b:{1})", fromNode.Label, toNode.Label) +
                    "WHERE a.UniqueId = $auid AND b.UniqueId = $buid " +
                    string.Format("CREATE (a)-[r: {0}]->(b) ", relType);
            }


            using (var session = _driver.Session())
            {
                var greeting = session.WriteTransaction(tx =>
                {
                    var result = props.Count > 0 ? tx.Run(query, props) : tx.Run(query);
                    return result;
                });

                Console.WriteLine(greeting);
            }

        }

        public long Push(Model.Node node, Dictionary<string, object> variables)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("props", variables);


            var nodeLabel = node.Label;
            var query = string.Format("CREATE (n:{0} $props) RETURN ID(n)", nodeLabel);

            IStatementResult retId;

            using (var session = _driver.Session())
            {
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
                        return (long)rtval;
                    }
                }

            }
            return -1;

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
