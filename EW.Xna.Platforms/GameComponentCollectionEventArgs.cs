using System;
using System.Collections.Generic;


namespace EW.Xna.Platforms
{
    public class GameComponentCollectionEventArgs:EventArgs
    {

        private IGameComponent _gameComponent;


        public GameComponentCollectionEventArgs(IGameComponent gameComponent)
        {
            _gameComponent = gameComponent;
        }

        public IGameComponent GameComponent
        {
            get { return _gameComponent; }
        }
    }
}