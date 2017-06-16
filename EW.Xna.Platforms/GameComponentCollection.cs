using System;
using System.Collections.ObjectModel;

namespace EW.Xna.Platforms
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameComponentCollection:Collection<IGameComponent>
    {
        public event EventHandler<GameComponentCollectionEventArgs> ComponentAdded;

        public event EventHandler<GameComponentCollectionEventArgs> ComponentRemoved;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        protected override void InsertItem(int index, IGameComponent item)
        {
            if (base.IndexOf(item) != -1)
                throw new ArgumentException("Cannot Add Same Component Multiple Times");

            base.InsertItem(index, item);

            if(item != null)
            {
                this.OnComponentAdded(new GameComponentCollectionEventArgs(item));
            }
        }

        protected override void RemoveItem(int index)
        {
            IGameComponent gameComponent = base[index];
            base.RemoveItem(index);
            if (gameComponent != null)
            {
                this.OnComponentRemoved(new GameComponentCollectionEventArgs(gameComponent));
            }
        }

        protected override void ClearItems()
        {
            for(int i = 0; i < base.Count; i++)
            {
                this.OnComponentRemoved(new GameComponentCollectionEventArgs(base[i]));
            }

            base.ClearItems();
        }

        private void OnComponentAdded(GameComponentCollectionEventArgs eventArgs)
        {
            if (ComponentAdded != null)
            {
                ComponentAdded(this, eventArgs);
            }
        }

        private void OnComponentRemoved(GameComponentCollectionEventArgs eventArgs)
        {
            if (ComponentRemoved != null)
            {
                ComponentRemoved(this, eventArgs);
            }
        }



    }
}