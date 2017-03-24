using System;


namespace EW.Mobile.Platforms.Graphics
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

        private readonly GraphicsDevice _device;
        public SpriteBatcher(GraphicsDevice device)
        {
            _device = device;
            _batchItemList = new SpriteBatchItem[InitialBatchSize];
            _batchItemCount = 0;

            for (int i = 0; i < InitialBatchSize; i++)
                _batchItemList[i] = new SpriteBatchItem();

        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public SpriteBatchItem CreateBatchItem()
        {
            if (_batchItemCount >= _batchItemList.Length)
            {

            }
            var item = _batchItemList[_batchItemCount];
            return item;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sortMode"></param>
        public unsafe void DrawBatch(SpriteSortMode sortMode,Effect effect)
        {

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
                        }
                    }
                }
            }
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
                _device
            }
        }

    }
}