using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLApps.MEPGraph.Model
{

    public class ElementType : Node
    {
        public string Category { get; set; }



    }

    public class ModelElement : Node
    {
        public string Category { get; set; }

    }

    public class Space : Node
    {

    }

    public class Project : Node
    {
        public string number { get; set; }
    }

    public class RevitModel : Node
    {
        public string uri { get; set; }
    }

    public class ForgeModel : Node
    {
        public string uri { get; set; }
    }

    public  class Surface : Node
    {
        public double area { get; set; }
    }

    public class VoidVolume : Node
    {

    }

    public class Wall : Node
    {

    }

    public class Door : Node
    {

    }

    public class Window : Node
    {

    }

    public class Ceiling : Node
    {

    }

    public class Column : Node
    {

    }


    public class Roof : Node
    {

    }   

    public class Floor : Node
    {

    }

    public class Section : Node
    {

    }

    public class Duct : Section
    {

    }

    public class Pipe : Section
    {

    }

    public class CableTray : Section
    {

    }

    public class Transition : Node
    {

    }

    public class DuctTransition : Transition
    {

    }
    public class PipeTransition : Transition
    {

    }

    public class CableTrayTransition : Transition
    {

    }

    public class Terminal : Node
    {

    }

    public class Accessory : Node
    {

    }

    public class DuctAccessory : Accessory
    {

    }
    public class PipeAccessory : Accessory
    {

    }
    public class Equipment : Node
    {

    }

    public class Level : Node
    {

    }


    public class System : Node
    {

    }

    public class Circuit : Node
    {

    }


    public class DBPanel : Node
    {

    }

    public class ElectricalLoad : Node
    {

    }

    public class ElectricalSource : Node
    {

    }

    public class ElectricalDevice : Node
    {

    }

    public class Data : Node
    {

    }

    public class Security : Node
    {

    }

    public class Safety : Node
    {

    }

    public class Sprinkler : Safety
    {

    }

    public class FireAlarm : Safety
    {

    }


    public class Lighting : Node
    {

    }
    public class Environment : Node
    {

    }


    public class Realisable : Node
    {

    }


    public class RealisableType : Node
    {

    }

}
