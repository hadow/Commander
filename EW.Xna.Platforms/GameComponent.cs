using System;


namespace EW.Xna.Platforms
{
    public interface IGameComponent
    {
        void Initialize();
    }

    public interface IUpdateable
    {
        void Update(GameTime gameTime);

        event EventHandler<EventArgs> EnabledChanged;

        event EventHandler<EventArgs> UpdateOrderChanged;

        bool Enabled { get; }

        int UpdateOrder { get; }
    }

    public class GameComponent:IGameComponent,IUpdateable,IComparable<GameComponent>,IDisposable
    {
        bool _enabled = true;

        int _updateOrder;

        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;

        public Game Game { get; private set; }


        public GameComponent(Game game)
        {
            this.Game = game;
        }
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    OnEnabledChanged(this, EventArgs.Empty);
                }
            }
        }

        public int UpdateOrder
        {
            get { return _updateOrder; }
            set
            {
                if(_updateOrder != value)
                {
                    _updateOrder = value;
                    OnUpdateOrderChanged(this, EventArgs.Empty);
                }
            }
        }

        public virtual void Initialize() { }

        public virtual void Update(GameTime gameTime) { }
        protected virtual void OnEnabledChanged(object sender,EventArgs args)
        {

        }

        protected virtual void OnUpdateOrderChanged(object sender,EventArgs args)
        {

        }

        public int CompareTo(GameComponent other)
        {
            return other.UpdateOrder - this.UpdateOrder;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) { }
    }
}