﻿using System.Collections.Generic;
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


        public Widget LoadWidget(WidgetArgs args,Widget parent,string w){

            MiniYamlNode ret;

            if (!widgets.TryGetValue(w, out ret))
                throw new InvalidDataException("Cannot find widget with Id '{0}' ".F(w));

            return LoadWidget(args, parent, ret);
        }

        public Widget LoadWidget(WidgetArgs args,Widget parent,MiniYamlNode node){

            if (!args.ContainsKey("modData"))
                args = new WidgetArgs(args) { { "modData", modData } };

            var widget = NewWidget(node.Key, args);

            if (parent != null)
                parent.AddChild(widget);

            if (node.Key.Contains("@"))
                FieldLoader.LoadField(widget, "Id", node.Key.Split('@')[1]);

            foreach (var child in node.Value.Nodes)
                if (child.Key != "Children")
                    FieldLoader.LoadField(widget, child.Key, child.Value.Value);

            widget.Initialize(args);

            foreach (var child in node.Value.Nodes)
                if (child.Key == "Children")
                    foreach (var c in child.Value.Nodes)
                        LoadWidget(args, widget, c);

            var logicNode = node.Value.Nodes.FirstOrDefault(n => n.Key == "Logic");
            var logic = logicNode == null ? null : logicNode.Value.ToDictionary();
            args.Add("logicArgs",logic);

            widget.PosInit(args);

            args.Remove("logicArgs");

            return widget;
        }


        static Widget NewWidget(string widgetType,WidgetArgs args){


            widgetType = widgetType.Split('@')[0];
            return WarGame.ModData.ObjectCreator.CreateObject<Widget>(widgetType + "Widget", args);
        }


    }
}