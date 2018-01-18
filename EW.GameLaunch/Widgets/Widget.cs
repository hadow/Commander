using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EW.Framework;
namespace EW.Widgets
{

    public static class UI
    {
        public static Widget Root = new ContainerWidget();

        public static long LastTickTime = WarGame.RunTime;

        static readonly Stack<Widget> WindowList = new Stack<Widget>();

        public static void CloseWindow()
        {

        }

        //public static Widget OpenWindow(string id)
        //{

        //}

        //public static Widget OpenWindow(string id,WidgetArgs args)
        //{
        //    var window = WarGame.ModData.WidgetLoader
        //}
    }

    public class ContainerWidget : Widget
    {

    }

    public class WidgetArgs : Dictionary<string, object>
    {
        public WidgetArgs() { }

        public WidgetArgs(Dictionary<string,object> args) : base(args) { }

        public void Add(string key,Action val) { base.Add(key, val); }
    }
    public abstract class Widget
    {

        public readonly List<Widget> Children = new List<Widget>();


        public string Id = null;
        public string X = "0";
        public string Y = "0";
        public string Width = "0";
        public string Height = "0";
        public string[] Logic = { };
        public ChromeLogic[] LogicObjects { get; private set; }
        public bool Visible = true;
        public bool IgnoreMouseOver;
        public bool IgnoreChildMouseOver;

        //
        public Rectangle Bounds;
        public Widget Parent = null;
        public Func<bool> IsVisible;

        public Widget()
        {
            IsVisible = () => Visible;
        }

        public Widget(Widget widget)
        {

        }

        public virtual Int2 ChildOrigin { get { return RenderOrigin; } }

        public virtual Int2 RenderOrigin
        {
            get
            {
                var offset = (Parent == null) ? Int2.Zero : Parent.ChildOrigin;
                return new Int2(Bounds.X, Bounds.Y) + offset;
            }
        }

        public virtual Rectangle RenderBounds
        {
            get
            {
                var ro = RenderOrigin;
                return new Rectangle(ro.X, ro.Y, Bounds.Width, Bounds.Height);
            }
        }

        public virtual Rectangle EventBounds { get { return RenderBounds; } }

        public virtual void Initialize(WidgetArgs args)
        {

        }
        public void PosInit(WidgetArgs args)
        {
            if (!Logic.Any())
                return;

            args["widget"] = this;

            LogicObjects = Logic.Select(l => WarGame.ModData.ObjectCreator.CreateObject<ChromeLogic>(l, args)).ToArray();

            args.Remove("widget");
        }

        public virtual void Tick() { }

        public virtual void TickOuter() { }

        public virtual void Draw() { }

        public virtual void DrawOuter() { }

        public virtual void Remove()
        {


        }


        public T Get<T>(string id) where T : Widget
        {
            var t = GetOrNull<T>(id);
            if (t == null)
                throw new InvalidOperationException("Widget {0} has no child {1} of type {2}".F(Id, id, typeof(T).Name));
            return t;
        }

        public T GetOrNull<T>(string id) where T : Widget
        {
            return (T)GetOrNull(id);
        }


        public Widget GetOrNull(string id)
        {
            if (Id == id)
                return this;

            foreach(var child in Children)
            {
                var w = child.GetOrNull(id);
                if (w != null)
                    return w;
            }
            return null;
        }
    }

    public class ChromeLogic : IDisposable
    {
        public virtual void Tick() { }

        public virtual void BecameHidden() { }

        public virtual void BecameVisible() { }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) { }

    }
}