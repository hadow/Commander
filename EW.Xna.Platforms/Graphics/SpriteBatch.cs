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
        Effect _spriteEffect;

        readonly EffectParameter _matrixTransform;
        readonly EffectPass _spritePass;
        Matrix _matrix;
        

        Vector2 _texCoordTL = new Vector2(0,0);
        Vector2 _texCoordBR = new Vector2(0,0);
        public SpriteBatch(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException("graphicsDevice");

            this.GraphicsDevice = graphicsDevice;

            _spriteEffect = new Effect(graphicsDevice, EffectResource.SpriteEffect.Bytecode);
            _matrixTransform = _spriteEffect.Parameters["MatrixTransform"];
            _spritePass = _spriteEffect.CurrentTechnique.Passes[0];
            _batcher = new SpriteBatcher(graphicsDevice);
            _beginCalled = false;
        }

        /// <summary>
        /// 开始绘制
        /// </summary>
        /// <param name="sortMode"></param>
        /// <param name="blendState"></param>
        /// <param name="samplerState"></param>
        /// <param name="depthStencilState"></param>
        /// <param name="rasterizerState"></param>
        /// <param name="effect"></param>
        /// <param name="transformMatrix"></param>
        public void Begin(SpriteSortMode sortMode = SpriteSortMode.Deferred,
                        BlendState blendState = null,
                        SamplerState samplerState = null,
                        DepthStencilState depthStencilState = null,
                        RasterizerState rasterizerState = null,
                        Effect effect = null,
                        Matrix? transformMatrix = null)
        {
            if (_beginCalled)
                throw new InvalidOperationException("Begin cannot be called agagin untile End has been sucessfully called.");

            _sortMode = sortMode;
            _blendState = blendState ?? BlendState.AlphaBlend;
            _samplerState = samplerState ?? SamplerState.LinearClamp;
            _depthStencilState = depthStencilState ?? DepthStencilState.None;
            _rasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;
            _effect = effect;
            _matrix = transformMatrix ?? Matrix.Identity;

            if(sortMode == SpriteSortMode.Immediate)
            {
                Setup();
            }

            _beginCalled = true;

        }

        /// <summary>
        /// 结束绘制
        /// </summary>
        public void End()
        {
            _beginCalled = false;

            if (_sortMode != SpriteSortMode.Immediate)
                Setup();
            _batcher.DrawBatch(_sortMode, null);

        }

        /// <summary>
        /// 
        /// </summary>
        void Setup()
        {
            var gd = GraphicsDevice;
            gd.BlendState = _blendState;
            gd.DepthStencilState = _depthStencilState;
            gd.RasterizerState = _rasterizerState;
            gd.SamplerStates[0] = _samplerState;

            var vp = gd.Viewport;

            Matrix projection;

            Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, 0, -1, out projection);

            Matrix.Multiply(ref _matrix, ref projection, out projection);

            _matrixTransform.SetValue(projection);
            _spritePass.Apply();
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
        /// <param name="position"></param>
        /// <param name="destinationRectangle"></param>
        /// <param name="sourceRectangle"></param>
        /// <param name="origin"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        /// <param name="color"></param>
        /// <param name="effects"></param>
        /// <param name="layerDepth"></param>
        public void Draw(Texture2D texture, Vector2? position = null,
                        Rectangle? destinationRectangle = null,
                        Rectangle? sourceRectangle = null,
                        Vector2? origin = null,
                         float rotation = 0f,
                         Vector2? scale = null,
                         Color? color = null,
                         SpriteEffects effects = SpriteEffects.None,
                         float layerDepth = 0f)
        {
            if (!color.HasValue)
                color = Color.White;
            if (!origin.HasValue)
                origin = Vector2.Zero;
            if (!scale.HasValue)
                scale = Vector2.One;

            if ((destinationRectangle.HasValue) == (position.HasValue))
                throw new InvalidOperationException("Expected drawRectangle or position,but received nither or both");
            else if (position != null)
                Draw(texture, (Vector2)position, sourceRectangle, (Color)color, rotation, (Vector2)origin, (Vector2)scale, effects, layerDepth);
        }


        public void Draw(Texture2D texture,Vector2 position,Rectangle? sourceRectangle,
                        Color color, float rotation,Vector2 origin,Vector2 scale,SpriteEffects effects,float layerDepth)
        {
            CheckValid(texture);

            var w = texture.Width * scale.X;
            var h = texture.Height * scale.Y;

            if (sourceRectangle.HasValue)
            {
                w = sourceRectangle.Value.Width * scale.X;
                h = sourceRectangle.Value.Height * scale.Y;
            }

            DrawInternal(texture, new Vector4(position.X, position.Y, w, h),
                sourceRectangle, color, rotation, origin * scale, effects, layerDepth, true);
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
        public void Draw(Texture2D texture,Vector2 postion,Rectangle? sourceRectangle,
            Color color,float rotation,Vector2 origin,
            float scale,SpriteEffects effects,float layerDepth)
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
        internal void DrawInternal(Texture2D texture,
            Vector4 destinationRectangle,
            Rectangle? sourceRectangle,
            Color color,float rotation,Vector2 origin,
            SpriteEffects effect,float depth,bool autoFlush)
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
                item.Set(destinationRectangle.X - origin.X,
                    destinationRectangle.Y - origin.Y, 
                    destinationRectangle.Z, 
                    destinationRectangle.W,
                    color, _texCoordTL, _texCoordBR, depth);
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