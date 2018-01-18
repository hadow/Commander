using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace EW.Widgets
{
    public class WidgetLoader
    {
        readonly Dictionary<string, MiniYamlNode> widgets = new Dictionary<string, MiniYamlNode>();
        readonly ModData modData;

        public WidgetLoader(ModData modData)
        {
            this.modData = modData;

            foreach(var file in modData.Manifest.ChromeLayout.Select(a=>MiniYaml.FromStream(modData.DefaultFileSystem.Open(a),a)))
                foreach(var w in file)
                {
                    var key = w.Key.Substring(w.Key.IndexOf('@') + 1);
                    if (widgets.ContainsKey(key))
                        throw new InvalidDataException("Widget has duplicate Key '{0}' at {1}".F(w.Key, w.Location));
                    widgets.Add(key, w);
                }
        }




    }
}