using System.Collections.Generic;

namespace HLApps.MEPGraph
{
    public class PendingNode
    {
        public PendingNode(Model.Node node)
        {
            Node = node;
            TempId = shortid.ShortId.Generate(7);
        }

        public long NodeId { get; internal set; }
        public string TempId { get; internal set; }
        public Model.Node Node { get; internal set; }

        public bool WasCommited { get; internal set; }

        public Dictionary<string, object> Variables { get; internal set; }
    }
}
