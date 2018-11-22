using System.Collections.Generic;

namespace HLApps.MEPGraph.Model
{
    public class Node
    {

        public string UniqueId { get; set; }

        public virtual string Label
        {
            get
            {
                return GetType().Name;
            }
        }

        public string Name { get; set; }

        Dictionary<string, object> _extendedProperties = new Dictionary<string, object>();
        public Dictionary<string, object> ExtendedProperties { get => _extendedProperties; }


        public virtual Dictionary<string, object> GetAllProperties()
        {
            var allProps = new Dictionary<string, object>();

            foreach(var kvp in ExtendedProperties)
            {
                allProps.Add(kvp.Key, kvp.Value);
            }

            if(!allProps.ContainsKey("Name")) allProps.Add("Name", Name);

            return allProps;

        }

    }

}
