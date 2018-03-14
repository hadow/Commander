using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EW.Framework;
using EW.Framework.Touch;
using EW.Graphics;
using EW.Support;
namespace EW.Widgets
{

    public static class UI
    {
        public static Widget Root = new ContainerWidget();

        public static long LastTickTime = WarGame.RunTime;

        public static Widget FocusWidget;


        static readonly Stack<Widget> WindowList = new Stack<Widget>();

        public static Widget CurrentWindow(){

            return WindowList.Count > 0 ? WindowList.Peek() : null;
        }   

        public static void Tick(){
            Root.TickOuter();
        }

        public static void Draw(){
            Root.DrawOuter();
        }

        public static bool HandleInput(GestureSample gs){

            var handled = false;

            if (FocusWidget != null && FocusWidget.HandleInputOuter(gs))
                handled = true;

            if (!handled && Root.HandleInputOuter(gs))
                handled = true;

            if(gs.GestureType == GestureType.FreeDrag)
            {
                GameViewPort.LastMousePos = gs.Position.ToInt2();
                GameViewPort.LastMoveRunTime = WarGame.RunTime;

            }

            return handled;
        }


        public static void PrepareRenderables(){Root.PrepareRenderablesOuter();}


        public static void CloseWindow()
        {
            if(WindowList.Count>0){
                var hidden = WindowList.Pop();

                Root.RemoveChild(hidden);

                if (hidden.LogicObjects != null)
                    foreach (var lo in hidden.LogicObjects)
                        lo.BecameHidden();


            }

            if(WindowList.Count>0){

                var restore = WindowList.Peek();

                Root.AddChild(restore);

                if (restore.LogicObjects != null)
                    foreach (var lo in restore.LogicObjects)
                        lo.BecameVisible();
            }


        }

        public static Widget OpenWindow(string id)
        {
            return OpenWindow(id, new WidgetArgs());
        }

        public static Widget OpenWindow(string id,WidgetArgs args)
        {
            var window = WarGame.ModData.WidgetLoader.LoadWidget(args, Root, id);
            if (WindowList.Count > 0)
                Root.HideChild(WindowList.Peek());
            WindowList.Push(window);
            return window;
        }

        public static void ResetAll(){
            Root.RemoveChildren();

            while (WindowList.Count > 0)
                CloseWindow();
        }
    }

    public class ContainerWidget : Widget
    {
        public readonly bool ClickThrough = true;

        public ContainerWidget(){}

        public ContainerWidget(ContainerWidget other):base(other){}

        public override bool HandleInput(GestureSample gs)
        {
            return !ClickThrough && EventBounds.Contains(gs.Position.ToInt2());
        }


        public override Widget Clone()
        {
            return new ContainerWidget(this);
        }
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
            Id = widget.Id;
            X = widget.X;
            Y = widget.Y;
            Width = widget.Width;
            Height = widget.Height;
            Logic = widget.Logic;
            Visible = widget.Visible;

            Bounds = widget.Bounds;
            Parent = widget.Parent;

            IsVisible = widget.IsVisible;

            foreach (var child in widget.Children)
                AddChild(child);
        }


        public virtual void Initialize(WidgetArgs args)
        {

            var parentBounds = (Parent == null)
                ? new Rectangle(0, 0, WarGame.Renderer.Resolution.Width, WarGame.Renderer.Resolution.Height)
                : Parent.Bounds;

            var substitutions = args.ContainsKey("substitutions") ? new Dictionary<string, int>((Dictionary<string, int>)args["substitutions"])
                : new Dictionary<string, int>();

            substitutions.Add("WINDOW_RIGHT", WarGame.Renderer.Resolution.Width);
            substitutions.Add("WINDOW_BOTTOM", WarGame.Renderer.Resolution.Height);
            substitutions.Add("PARENT_RIGHT", parentBounds.Width);
            substitutions.Add("PARENT_LEFT", parentBounds.Left);
            substitutions.Add("PARENT_TOP", parentBounds.Top);
            substitutions.Add("PARENT_BOTTOM", parentBounds.Height);

            var width = Evaluator.Evaluate(Width, substitutions);
            var height = Evaluator.Evaluate(Height, substitutions);

            substitutions.Add("WIDTH", width);
            substitutions.Add("HEIGHT", height);

            Bounds = new Rectangle(Evaluator.Evaluate(X, substitutions),
                                   Evaluator.Evaluate(Y, substitutions),
                                  width,
                                   height);

        }
        public void PosInit(WidgetArgs args)
        {
            if (!Logic.Any())
                return;

            args["widget"] = this;

            LogicObjects = Logic.Select(l => WarGame.ModData.ObjectCreator.CreateObject<ChromeLogic>(l, args)).ToArray();

            args.Remove("widget");
        }


        public virtual Widget Clone()
        {

            throw new InvalidOperationException("Widget type '{0}' is not cloneable ".F(GetType().Name));
        }
        public virtual bool TakeFocus(GestureSample gs){

            if (HasFocus)
                return true;

            if (UI.FocusWidget != null && !UI.FocusWidget.YieldFocus(gs))
                return false;
            UI.FocusWidget = this;
            return true;
                
        }


        public virtual bool YieldFocus(GestureSample gs){
            if (UI.FocusWidget == this)
                UI.FocusWidget = null;
            return true;
        }

        public bool HasFocus{
            get{
                return UI.FocusWidget == this;
            }
        }

        public virtual Rectangle GetEventBounds(){

            var bounds = EventBounds;

            foreach(var child in Children)
            {
                if(child.IsVisible())
                {
                    var childBounds = child.GetEventBounds();
                    if(childBounds != Rectangle.Empty)
                    {
                        bounds = Rectangle.Union(bounds, childBounds);
                    }
                }
            }

            return bounds;
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



        public virtual void Tick() { }

        public virtual void TickOuter() { 

            if(IsVisible()){

                Tick();
                foreach (var child in Children)
                    child.TickOuter();

                if (LogicObjects != null)
                    foreach (var lo in LogicObjects)
                        lo.Tick();
            }
        
        }


        public virtual void PrepareRenderables(){}

        public virtual void PrepareRenderablesOuter(){
            if(IsVisible())
            {
                PrepareRenderables();
                foreach (var child in Children)
                    child.PrepareRenderablesOuter();
            }
        }

        public virtual void Draw() { }

        public virtual void DrawOuter() {

            if(IsVisible())
            {
                Draw();
                foreach (var child in Children)
                    child.DrawOuter();
                
            }
        
        }


        public virtual bool HandleInput(GestureSample gs) { return false; }
        public bool HandleInputOuter(GestureSample gs){

            if (!(HasFocus || (IsVisible() && GetEventBounds().Contains(gs.Position.ToInt2()))))
                return false;


            foreach (var child in Children.OfType<Widget>().Reverse())
                if (child.HandleInputOuter(gs))
                    return true;

            return HandleInput(gs);
        }


        public virtual void AddChild(Widget child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        public virtual void RemoveChildren()
        {
            while (Children.Count > 0)
                RemoveChild(Children[Children.Count - 1]);
        }

        public virtual void RemoveChild(Widget child){
            if(child!=null){
                Children.Remove(child);
                child.Removed();
            }
        }

        public virtual void HideChild(Widget child){
            if(child!=null){
                Children.Remove(child);
                child.Hidden();
            }
        }



        public virtual void Hidden(){
            foreach (var c in Children.OfType<Widget>().Reverse())
                c.Hidden();
            
        }
        public virtual void Removed(){

            foreach (var c in Children.OfType<Widget>().Reverse())
                c.Removed();

            if (LogicObjects != null)
                foreach (var lo in LogicObjects)
                    lo.Dispose();
            
        }

        public Widget Get(string id){
            return Get<Widget>(id);
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