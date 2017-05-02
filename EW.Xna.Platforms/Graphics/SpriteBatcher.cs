using System;


namespace EW.Xna.Platforms.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    internal class SpriteBatcher
    {

        private const int InitialBatchSize = 256;

        private const int MaxBatchSize = short.MaxValue / 6;

        private SpriteBatchItem[] _batchItemList;
        private VertexPositionColorTexture[] _vertexArray;
        private int _batchItemCount;

        private short[] _index;
        private readonly GraphicsDevice _device;
        public SpriteBatcher(GraphicsDevice device)
        {
            _device = device;
            _batchItemList = new SpriteBatchItem[InitialBatchSize];
            _batchItemCount = 0;

            for (int i = 0; i < InitialBatchSize; i++)
                _batchItemList[i] = new SpriteBatchItem();

            EnsureArrayCapacity(InitialBatchSize);
        }



        private unsafe void EnsureArrayCapacity(int numBatchItems)
        {
            int needCapacity = 6 * numBatchItems;
            if (_index != null && needCapacity <= _index.Length)
                return;

            short[] newIndex = new short[6 * numBatchItems];
            int start = 0;
            if (_index != null)
            {
                _index.CopyTo(newIndex, 0);
                start = _index.Length / 6;
            }

            fixed(short* indexFixedPtr = newIndex)
            {
                var indexPtr = indexFixedPtr + (start * 6);

                for(var i = start; i < numBatchItems; i++, indexPtr += 6)
                {
                    *(indexPtr + 0) = (short)(i * 4);
                    *(indexPtr + 1) = (short)(i * 4 + 1);
                    *(indexPtr + 2) = (short)(i * 4 + 2);

                    *(indexPtr + 3) = (short)(i * 4 + 1);
                    *(indexPtr + 4) = (short)(i * 4 + 3);
                    *(indexPtr + 5) = (short)(i * 4 + 2);
                }
            }
            _index = newIndex;

            _vertexArray = new VertexPositionColorTexture[4 * numBatchItems];
        }



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public SpriteBatchItem CreateBatchItem()
        {
            if (_batchItemCount >= _batchItemList.Length)
            {
                var oldSize = _batchItemList.Length;
                var newSize = oldSize + oldSize / 2;
                newSize = (newSize + 63) & (~63);
                Array.Resize(ref _batchItemList, newSize);
                for(int i = oldSize; i < newSize; i++)
                {
                    _batchItemList[i] = new SpriteBatchItem();
                }
                EnsureArrayCapacity(Math.Min(newSize, MaxBatchSize));
            }
            var item = _batchItemList[_batchItemCount++];
            return item;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sortMode"></param>
        public unsafe void DrawBatch(SpriteSortMode sortMode,Effect effect)
        {
            if (effect != null && effect.IsDisposed)
                throw new ObjectDisposedException("effect");

            if (_batchItemCount == 0)
                return;

            switch (sortMode)
            {
                case SpriteSortMode.BackToFront:
                case SpriteSortMode.Texture:
                case SpriteSortMode.FrontToBack:
                    Array.Sort(_batchItemList, 0, _batchItemCount);
                    break;

            }

            int batchIndex = 0;
            int batchCount = _batchItemCount;

            unchecked
            {
                _device._graphicsMetrics._spriteCount += batchCount;
            }
            while (batchCount > 0)
            {
                var startIndex = 0;
                var index = 0;
                Texture2D tex = null;
                int numBatchesToProcess = batchCount;
                if (numBatchesToProcess > MaxBatchSize)
                {
                    numBatchesToProcess = MaxBatchSize;
                }

                fixed(VertexPositionColorTexture* vertexArrayFixedPtr = _vertexArray)
                {
                    var vertexArrayPtr = vertexArrayFixedPtr;
                    for(int i = 0; i < numBatchesToProcess; i++, batchIndex++, index += 4, vertexArrayPtr += 4)
                    {
                        SpriteBatchItem batchItem = _batchItemList[batchIndex];
                        var shouldFlush = !ReferenceEquals(batchItem.Texture, tex);
                        if (shouldFlush)
                        {
                            FlushVertexArray(startIndex, index, effect, tex);
                            tex = batchItem.Texture;
                            startIndex = index = 0;
                            vertexArrayPtr = vertexArrayFixedPtr;
                            _device.Textures[0] = tex;
                        }

                        *(vertexArrayPtr + 0) = batchItem.vertexTL;
                        *(vertexArrayPtr + 1) = batchItem.vertexTR;
                        *(vertexArrayPtr + 2) = batchItem.vertexBL;
                        *(vertexArrayPtr + 3) = batchItem.vertexBR;

                        batchItem.Texture = null;
                    }
                }

                FlushVertexArray(startIndex, index, effect, tex);
                batchCount -= numBatchesToProcess;
            }

            //return items to the pool
            _batchItemCount = 0;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="effect"></param>
        /// <param name="texture"></param>
        private void FlushVertexArray(int start,int end,Effect effect,Texture texture)
        {
            if (start == end)
                return;

            var vertexCount = end - start;

            if (effect != null)
            {

            }
            else
            {
                //simply render
                _device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _vertexArray, 0, vertexCount, _index, 0, (vertexCount / 4) * 2, VertexPositionColorTexture.VertexDeclaration);
            }
        }

    }
}