using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using HLApps.MEPGraph.Model;
using HLApps.Revit.Parameters;

namespace HLApps.Revit.Graph
{
    public static class MEPGraphUtils
    {
        public static object RevitToGraphValue(HLRevitElementData elmData)
        {
            var val = elmData.Value;
            if (val is ElementId) return (val as ElementId).IntegerValue;

            if (val is Element) return (val as Element).Name;

            return val;
        }

        public static Dictionary<string, object> GetNodePropsWithElementProps(Node node, Element elm)
        {
            var elmParms = node.GetAllProperties();


            if (elm != null && elm.Location is Autodesk.Revit.DB.LocationPoint)
            {
                var lpt = (elm.Location as Autodesk.Revit.DB.LocationPoint);
                var insPt = lpt.Point;
                if (!elmParms.ContainsKey("LocationX")) elmParms.Add("LocationX", insPt.X);
                if (!elmParms.ContainsKey("LocationY")) elmParms.Add("LocationY", insPt.Y);
                if (!elmParms.ContainsKey("LocationZ")) elmParms.Add("LocationZ", insPt.Z);
                //if (!elmParms.ContainsKey("LocationRotation")) elmParms.Add("LocationRotation", lpt.Rotation);
            }
            else if (elm != null && elm.Location is Autodesk.Revit.DB.LocationCurve)
            {
                //just start and end points for now
                var insPt = (elm.Location as Autodesk.Revit.DB.LocationCurve).Curve.GetEndPoint(0);
                if (!elmParms.ContainsKey("LocationX")) elmParms.Add("LocationX", insPt.X);
                if (!elmParms.ContainsKey("LocationY")) elmParms.Add("LocationY", insPt.Y);
                if (!elmParms.ContainsKey("LocationZ")) elmParms.Add("LocationZ", insPt.Z);

                insPt = (elm.Location as Autodesk.Revit.DB.LocationCurve).Curve.GetEndPoint(1);
                if (!elmParms.ContainsKey("LocationX2")) elmParms.Add("LocationX2", insPt.X);
                if (!elmParms.ContainsKey("LocationY2")) elmParms.Add("LocationY2", insPt.Y);
                if (!elmParms.ContainsKey("LocationZ2")) elmParms.Add("LocationZ2", insPt.Z);
            }


            foreach (var param in elm.Parameters.OfType<Autodesk.Revit.DB.Parameter>())
            {
                var hp = new HLRevitParameter(param);
                var val = RevitToGraphValue(hp);

                if (!elmParms.ContainsKey(param.Definition.Name))
                {
                    elmParms.Add(param.Definition.Name, val);
                }

                if (!elmParms.ContainsKey(param.Definition.Name))
                {
                    elmParms.Add(param.Definition.Name, val);
                }
            }

            return elmParms;
        }
   
        public static Dictionary<string, object> GetEdgeProps(Element elm)
        {
            var eprops = new Dictionary<string, object>();
            if (elm != null)
            {
                eprops.Add("UniqueId", elm.UniqueId);
                eprops.Add("ModelId", elm.Id.IntegerValue);
            }

            return eprops;

        }
    }
}
