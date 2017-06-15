using System;


namespace EW.Xna.Platforms
{
    public interface IDrawable
    {
        int DrawOrder { get; }

        bool Visible { get; }

        event EventHandler<EventArgs> DrawOrderChanged;

        event EventHandler<EventArgs> VisibleChanged;

    }
    public class DrawableGameComponent:GameComponent,IDrawable
    {

        private int _drawOrder;

        private bool _visible = true;

        public event EventHandler<EventArgs> DrawOrderChanged;
        public event EventHandler<EventArgs> VisibleChanged;
        public int DrawOrder { get { return _drawOrder; }
            set {
                if(_drawOrder != value)
                {
                    _drawOrder = value;
                    OnDrawOrderChanged(this, EventArgs.Empty);
                }
            } }

        public bool Visible
        {
            get { return _visible; }
            set
            {
                if(_visible != value)
                {
                    _visible = value;
                    OnVisibleChanged(this, EventArgs.Empty);
                }
            }
        }

        protected virtual void OnVisibleChanged(object sender,EventArgs args)
        {

        }

        protected virtual void OnDrawOrderChanged(object sender,EventArgs args)
        {

        }
    }
}