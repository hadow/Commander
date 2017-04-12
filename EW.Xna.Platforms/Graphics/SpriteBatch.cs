using System;


namespace EW.Xna.Platforms.Graphics
{

    /// <summary>
    /// 
    /// </summary>
    public enum SpriteEffects
    {
        None = 0,
        FlipHorizontally = 1,
        FlipVertically = 2,
    }


    /// <summary>
    /// 
    /// </summary>
    public class SpriteBatch:GraphicsResource
    {
        bool _beginCalled;

        readonly SpriteBatcher _batcher;

        SpriteSortMode _sortMode;

        BlendState _blendState;
        SamplerState _samplerState;
        DepthStencilState _depthStencilState;
        RasterizerState _rasterizerState;

        Effect _effect;








        Vector2 _texCoordTL = new Vector2(0,0);
        Vector2 _texCoordBR = new Vector2(0,0);
        public SpriteBatch(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException("graphicsDevice");

            this.GraphicsDevice = graphicsDevice;

            _batcher = new SpriteBatcher(graphicsDevice);
            _beginCalled = false;
        }

        public void Begin(SpriteSortMode sortMode = SpriteSortMode.Deferred,BlendState blendState = null,
            SamplerState samplerState = null,DepthStencilState depthStencilState = null,RasterizerState rasterizerState = null)
        {



        }

        public void End()
        {
            _beginCalled = false;

            if (_sortMode != SpriteSortMode.Immediate)
                Setup();
            _batcher.DrawBatch(_sortMode, null);    

        }

        void Setup()
        {

        }



        void CheckValid(Texture2D texture)
        {
            if (texture == null)
                throw new ArgumentException("texture");
            if (!_beginCalled)
                throw new InvalidOperationException("Draw was called,but Begin has not yet been called,Begin must be called successfully before you can call Draw.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="postion"></param>
        /// <param name="sourceRectangle"></param>
        /// <param name="color"></param>
        /// <param name="rotation"></param>
        /// <param name="origin"></param>
        /// <param name="scale"></param>
        /// <param name="effects"></param>
        /// <param name="layerDepth"></param>
        public void Draw(Texture2D texture,Vector2 postion,Rectangle? sourceRectangle,Color color,float rotation,Vector2 origin,float scale,SpriteEffects effects,float layerDepth)
        {
            CheckValid(texture);

            var w = texture.width * scale;
            var h = texture.height * scale;

            if (sourceRectangle.HasValue)
            {
                w = sourceRectangle.Value.Width * scale;
                h = sourceRectangle.Value.Height * scale;
            }

            DrawInternal(texture, new Vector4(postion.X, postion.Y, w, h),sourceRectangle,color,rotation,origin,effects,layerDepth,true);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="destinationRectangle"></param>
        /// <param name="sourceRectangle"></param>
        /// <param name="color"></param>
        /// <param name="rotation"></param>
        /// <param name="origin"></param>
        /// <param name="effect"></param>
        /// <param name="depth"></param>
        /// <param name="autoFlush"></param>
        internal void DrawInternal(Texture2D texture,Vector4 destinationRectangle,Rectangle? sourceRectangle,Color color,float rotation,Vector2 origin,SpriteEffects effect,float depth,bool autoFlush)
        {
            var item = _batcher.CreateBatchItem();
            item.Texture = texture;
            switch (_sortMode)
            {
                case SpriteSortMode.Texture:
                    item.SortKey = texture.SortingKey;
                    break;
                case SpriteSortMode.FrontToBack:
                    item.SortKey = depth;
                    break;
                case SpriteSortMode.BackToFront:
                    item.SortKey = -depth;
                    break;
            }

            if (sourceRectangle.HasValue)
            {

            }
            else
            {
                _texCoordTL.X = 0;
                _texCoordTL.Y = 0;
                _texCoordBR.X = 1;
                _texCoordBR.Y = 1;   
            }

            if(rotation == 0)
            {
                item.Set(destinationRectangle.X - origin.X, destinationRectangle.Y - origin.Y, destinationRectangle.Z, destinationRectangle.W, color, _texCoordTL, _texCoordBR, depth);
            }
            else
            {

            }

            if (autoFlush)
            {
                FlushIfNeeded();
            }


        }


        /// <summary>
        /// 
        /// </summary>
        internal void FlushIfNeeded()
        {
            if(_sortMode == SpriteSortMode.Immediate)
            {
                _batcher.DrawBatch(_sortMode, _effect);
            }
        }








    }
}