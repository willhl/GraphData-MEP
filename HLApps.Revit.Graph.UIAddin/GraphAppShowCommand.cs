using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using HLApps.Revit.Graph.UIAddin.ViewModel;


namespace HLApps.Revit.Graph.UIAddin
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class GraphAppShowCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData cmdData, ref string message, ElementSet elements)
        {
            Document rDoc = cmdData.Application.ActiveUIDocument.Document;
            var gdApp = GraphApp.Instance;
            var publisher = new RevitToGraphPublisher(rDoc);
            GraphAppViewModel gvm = new GraphAppViewModel(publisher, gdApp);
            gdApp.GraphAppWindow = new GraphAppWindow(gvm);
            gdApp.GraphAppWindow.ShowDialog();

            return Result.Succeeded;
        }
    }
}