using System.Collections.Generic;
using HLApps.Revit.Geometry.Octree;
using HLApps.Revit.Geometry;
using HLApps.Revit.Graph.Parsers;

namespace HLApps.Revit.Graph
{

    public class MEPPathWriteCache
    {
        public BoundsOctree<GeometrySegment> geoCache;
        public BoundsOctreeElementWriter geoCacheWriter;
        public PointOctree<ConnectorPointGeometrySegment> connectorsCache;
        public PointOctree<FaceIntersectRay> rayhitCache;
        public HashSet<int> ParsedElements;
        public double MaxDepth;
    }
}
    
