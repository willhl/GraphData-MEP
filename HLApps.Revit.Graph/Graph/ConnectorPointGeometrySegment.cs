
using Autodesk.Revit.DB;
using HLApps.Revit.Geometry;

namespace HLApps.Revit.Graph
{

    public class ConnectorPointGeometrySegment : PointGeometrySegment
    {
        public ConnectorPointGeometrySegment(ElementId element, XYZ point, int Connectorindex) : base(element, point)
        {
            ConnectorIndex = Connectorindex;
        }

        public int ConnectorIndex { get; set; }
    }
}
