using System.Collections.Generic;
using HLApps.MEPGraph.Model;

namespace HLApps.MEPGraph
{
    public interface IGraphDBClient
    {
        void Dispose();
        PendingNode Push(Node node, Dictionary<string, object> variables);
        void Relate(PendingNode fromNodeId, PendingNode toNodeId, MEPEdgeTypes relType, Dictionary<string, object> variables);
        void Relate(Node fromNode, Node toNode, string relType, Dictionary<string, object> variables);

        void Commit();
    }
}