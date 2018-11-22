using System.Collections.Generic;
using System.Linq;

using HLApps.Revit.Utils;
using HLApps.MEPGraph;
using HLApps.MEPGraph.Model;


namespace HLApps.Revit.Graph
{
    public class MEPGraphWriter
    {
        IGraphDBClient _gdbClient;
        public MEPGraphWriter(IGraphDBClient gdbClient)
        {
            _gdbClient = gdbClient;
        }


        public void Write(MEPRevitGraph mepGraph, Autodesk.Revit.DB.Document rootDoc)
        {

            var track = new Dictionary<MEPRevitNode, long>();

            var models = new Dictionary<string, long>();
            var types = new Dictionary<string, long>();
            var levels = new Dictionary<string, long>();

            var rootModelNode = new RevitModel();
            var rootModelIdent = DocUtils.GetDocumentIdent(rootDoc);
            rootModelNode.Name = rootDoc.PathName;
            rootModelNode.ExtendedProperties.Add("Identity", rootModelIdent);
            rootModelNode.ExtendedProperties.Add("DateTimeStamp", System.DateTime.Now);
            var rootparams = MEPGraphUtils.GetNodePropsWithElementProps(rootModelNode, rootDoc.ProjectInformation);
            var seid = _gdbClient.Push(rootModelNode, rootparams);
            models.Add(rootModelIdent, seid);



            //add the nodes
            foreach (var mepNode in mepGraph.Nodes)
            {

                var npNode = mepNode.AsAbstractNode;
                var elmAbParams = npNode.GetAllProperties();


                if (!string.IsNullOrEmpty(mepNode.OrginDocIdent))
                {
                    var elmNode = mepNode.AsElementNode;
                    var elmParms = elmNode.GetAllProperties();

                    var elmdoc = DocUtils.GetDocument(mepNode.OrginDocIdent, rootDoc.Application);
                    var elm = elmdoc.GetElement(new Autodesk.Revit.DB.ElementId(mepNode.OriginId));
                    if (elm != null)
                    {

                        elmParms = MEPGraphUtils.GetNodePropsWithElementProps(elmNode, elm);
                        elmAbParams = MEPGraphUtils.GetNodePropsWithElementProps(npNode, elm);
                    }

                    var atid = _gdbClient.Push(npNode, elmAbParams);
                    track.Add(mepNode, atid);

                    var elmid = _gdbClient.Push(elmNode, elmParms);

                    //relate the element node to the abstracted model node
                    _gdbClient.Relate(atid, elmid, MEPEdgeTypes.REALIZED_BY, null);


                    var modelId = 0L;
                    //create up model nodes
                    if (models.ContainsKey(mepNode.OrginDocIdent))
                    {
                        modelId = models[mepNode.OrginDocIdent];
                    }
                    else
                    {
                        var modelNode = new RevitModel();
                        modelNode.ExtendedProperties.Add("Identity", mepNode.OrginDocIdent);
                        var mparams = modelNode.GetAllProperties();

                        var ldoc = DocUtils.GetDocument(mepNode.OrginDocIdent, rootDoc.Application);
                        if (ldoc != null)
                        {
                            mparams  = MEPGraphUtils.GetNodePropsWithElementProps(modelNode, ldoc.ProjectInformation);
                        }

                        modelId = _gdbClient.Push(modelNode, mparams);
                        models.Add(mepNode.OrginDocIdent, modelId);
                    }

                    var elmedgeProps = MEPGraphUtils.GetEdgeProps(elm);
                    //connect up model node to element node
                    _gdbClient.Relate(elmid, modelId, MEPEdgeTypes.IS_IN, elmedgeProps);

                    Autodesk.Revit.DB.Element typeElm = null;

                   
                    if (elm is Autodesk.Revit.DB.FamilyInstance)
                    {
                        typeElm = (elm as Autodesk.Revit.DB.FamilyInstance).Symbol;

                    }
                    else
                    {
                        var mpType = elm.GetTypeId();
                        typeElm = elmdoc.GetElement(mpType);
                    }

                    /*
                    if (elm is Autodesk.Revit.DB.MEPCurve)
                    {
                        var mpType = (elm as Autodesk.Revit.DB.MEPCurve).GetTypeId();
                        typeElm = elmdoc.GetElement(mpType);
                    }
                    else if (elm is Autodesk.Revit.DB.RoofBase)
                    {
                        var mpType = (elm as Autodesk.Revit.DB.RoofBase).GetTypeId();
                        typeElm = elmdoc.GetElement(mpType);
                    }
                    else if (elm is Autodesk.Revit.DB.Floor)
                    {
                        var mpType = (elm as Autodesk.Revit.DB.Floor).GetTypeId();
                        typeElm = elmdoc.GetElement(mpType);
                    }
                    else if (elm is Autodesk.Revit.DB.Ceiling)
                    {
                        var mpType = (elm as Autodesk.Revit.DB.Ceiling).GetTypeId();
                        typeElm = elmdoc.GetElement(mpType);
                    }
                    else if (elm is Autodesk.Revit.DB.Wall)
                    {
                        var mpType = (elm as Autodesk.Revit.DB.Wall).GetTypeId();
                        typeElm = elmdoc.GetElement(mpType);
                    }
                    else if (elm is Autodesk.Revit.DB.SpatialElement)
                    {
                        var mpType = (elm as Autodesk.Revit.DB.SpatialElement).GetTypeId();
                        typeElm = elmdoc.GetElement(mpType);
                    }
                    else if (elm is Autodesk.Revit.DB.Electrical.ElectricalSystem)
                    {
                        var mpType = (elm as Autodesk.Revit.DB.Electrical.ElectricalSystem).GetTypeId();
                        typeElm = elmdoc.GetElement(mpType);
                    }
                    */

                    //create type nodes
                    if (typeElm != null)
                    {
                        var tsId = 0L;
                        if (!types.ContainsKey(typeElm.UniqueId))
                        {
                            var edgeProps = MEPGraphUtils.GetEdgeProps(typeElm);
                            var tsNode = new ElementType();
                            tsNode.Name = typeElm.Name;
                            var tsprops = MEPGraphUtils.GetNodePropsWithElementProps(tsNode, typeElm);

                            tsId = _gdbClient.Push(tsNode, tsprops);
                            types.Add(typeElm.UniqueId, tsId);

                            var tselmNode = new ModelElement();
                            var tselmId = _gdbClient.Push(tselmNode, tsprops);

                            _gdbClient.Relate(tsId, tselmId, MEPEdgeTypes.REALIZED_BY, null);
                            _gdbClient.Relate(tselmId, modelId, MEPEdgeTypes.IS_IN, edgeProps);
                        }
                        else
                        {
                            tsId = types[typeElm.UniqueId];
                        }                      
                        _gdbClient.Relate(atid, tsId, MEPEdgeTypes.IS_OF, null);
                        
                    }



                    //create level nodes
                    var lvl = elmdoc.GetElement(new Autodesk.Revit.DB.ElementId(mepNode.LevelId));
                    if (lvl != null)
                    {
                        var edgeProps = MEPGraphUtils.GetEdgeProps(lvl);
                        var lvlId = 0L;
                        if (!levels.ContainsKey(lvl.UniqueId))
                        {
                            var lvlNode = new Level();
                            lvlNode.Name = lvl.Name;
                            var lvlprops = MEPGraphUtils.GetNodePropsWithElementProps(lvlNode, lvl);
                            lvlId = _gdbClient.Push(lvlNode, lvlprops);
                            levels.Add(lvl.UniqueId, lvlId);
                            _gdbClient.Relate(lvlId, modelId, MEPEdgeTypes.IS_IN, edgeProps);
                        }
                        else
                        {
                            lvlId = levels[lvl.UniqueId];
                        }
                       
                        _gdbClient.Relate(atid, lvlId, MEPEdgeTypes.IS_ON, null);
                        
                    }

                }
                else
                {
                    var modelId = _gdbClient.Push(npNode, elmAbParams);
                    track.Add(mepNode, modelId);
                }


            }

            //now add the adjacencies 
            foreach (var mepEdge in mepGraph.Edges)
            {
                if (!track.ContainsKey(mepEdge.ThisNode))
                {
                    continue;
                }

                if (!track.ContainsKey(mepEdge.NextNode))
                {
                    continue;
                }

                var nid1 = track[mepEdge.ThisNode];
                var nid2 = track[mepEdge.NextNode];

                var edPArams = new Dictionary<string, object>();
                foreach (var wkvp in mepEdge.Weights)
                {
                    edPArams.Add(wkvp.Key, wkvp.Value);
                }


                _gdbClient.Relate(nid1, nid2, mepEdge.AsNodeEdge.EdgeType, edPArams);
            }


            //add systems and connections
            foreach (var system in mepGraph.Systems)
            {
                
                var sysNode = new MEPGraph.Model.System();

                var syselm = rootDoc.GetElement(new Autodesk.Revit.DB.ElementId(system));
                var srops = MEPGraphUtils.GetNodePropsWithElementProps(sysNode, syselm);
                var sysNodeId = _gdbClient.Push(sysNode, srops);

                var tselmNode = new ModelElement();
                tselmNode.ExtendedProperties.Add("UniqueId", syselm.UniqueId);

                var emprops = MEPGraphUtils.GetNodePropsWithElementProps(tselmNode, syselm);
                var tselmId = _gdbClient.Push(tselmNode, emprops);
                _gdbClient.Relate(sysNodeId, tselmId, MEPEdgeTypes.REALIZED_BY, null);
                var edgeProps = MEPGraphUtils.GetEdgeProps(syselm);
                _gdbClient.Relate(tselmId, seid, MEPEdgeTypes.IS_IN, edgeProps);


                var stypeId = syselm.GetTypeId();
                var typeElm = rootDoc.GetElement(stypeId);
                if (typeElm != null)
                {
                    var tsId = 0L;
                    if (!types.ContainsKey(typeElm.UniqueId))
                    {
                        var stypeedgeProps = MEPGraphUtils.GetEdgeProps(typeElm);
                        var tsNode = new ElementType();
                        tsNode.Name = typeElm.Name;
                        var tsprops = MEPGraphUtils.GetNodePropsWithElementProps(tsNode, typeElm);

                        tsId = _gdbClient.Push(tsNode, tsprops);
                        types.Add(typeElm.UniqueId, tsId);

                        var sysTypeelmNode = new ModelElement();
                        var sysTypeelmId = _gdbClient.Push(sysTypeelmNode, tsprops);

                        _gdbClient.Relate(tsId, sysTypeelmId, MEPEdgeTypes.REALIZED_BY, null);
                        _gdbClient.Relate(tselmId, seid, MEPEdgeTypes.IS_IN, stypeedgeProps);
                    }
                    else
                    {
                        tsId = types[typeElm.UniqueId];
                    }

                    _gdbClient.Relate(sysNodeId, tsId, MEPEdgeTypes.IS_OF, null);

                }

                var snodes = mepGraph.GetAllNodesForSystem(system);
                foreach (var snd in snodes)
                {
                    if (!track.ContainsKey(snd))
                    {
                        continue;
                    }

                    var rid = track[snd];
                    _gdbClient.Relate(rid, sysNodeId, MEPEdgeTypes.ABSTRACTED_BY, null);
                }

            }
      
        }

        

    }
}
