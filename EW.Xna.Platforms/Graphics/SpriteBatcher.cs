using System;


namespace EW.Xna.Platforms.Graphics
{
    /// <summary>
    /// 该类通过创建用于绘制精灵纹理的三角形镶嵌来处理批处理项目进入GPU的排队。
    /// 该类支持int.MaxValue要批量的精灵数，并将它们处理为short.MaxValue组（对于发送到GPU的顶点数量，以6为单位）。
    /// </summary>
    internal class SpriteBatcher
    {

        private const int InitialBatchSize = 256;

        /// <summary>
        /// The maximum number of batch items that can be processed per iteration
        /// (6= 4 vertices unique and 2 shared,per quad)
        /// </summary>
        private const int MaxBatchSize = short.MaxValue / 6;

        /// <summary>
        /// The list of batch items to process
        /// </summary>
        private SpriteBatchItem[] _batchItemList;

        private VertexPositionColorTexture[] _vertexArray;
        /// <summary>
        /// 指向_batchItemList中下一个可用SpriteBatchItem的索引指针。
        /// </summary>
        private int _batchItemCount;

        /// <summary>
        /// Vertex index array,The values in this array never change
        ///(顶点索引数组。该数组中的值从不改变.)
        /// </summary>
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


        /// <summary>
        /// Resize and recreate the missing indices for the index and vertex position color buffers.
        /// (调整索引和顶点位置颜色缓冲区的缺失索引并重新创建索引。)
        /// </summary>
        /// <param name="numBatchItems"></param>
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
                    //Triangle 1
                    *(indexPtr + 0) = (short)(i * 4);
                    *(indexPtr + 1) = (short)(i * 4 + 1);
                    *(indexPtr + 2) = (short)(i * 4 + 2);

                    //Triangle 2
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
        /// Sorts the batch items and then groups batch drawing into maximal allowed batch sets that do not overflow the 16 bit array indices for vertices.
        /// 
        /// </summary>
        /// <param name="sortMode"></param>
        public unsafe void DrawBatch(SpriteSortMode sortMode,Effect effect)
        {
            if (effect != null && effect.IsDisposed)
                throw new ObjectDisposedException("effect");

            if (_batchItemCount == 0)
                return;

            //sort the batch items
            switch (sortMode)
            {
                case SpriteSortMode.BackToFront:
                case SpriteSortMode.Texture:
                case SpriteSortMode.FrontToBack:
                    Array.Sort(_batchItemList, 0, _batchItemCount);
                    break;

            }

            //Determine how many iterations through the drawing code we need to make
            int batchIndex = 0;
            int batchCount = _batchItemCount;

            unchecked
            {
                _device._graphicsMetrics._spriteCount += batchCount;
            }
            //Iterate through the batches,doing short.MaxValue sets of vertices only
            while (batchCount > 0)
            {
                // setup the vertexArray array;
                var startIndex = 0;
                var index = 0;
                Texture2D tex = null;

                int numBatchesToProcess = batchCount;
                if (numBatchesToProcess > MaxBatchSize)
                {
                    numBatchesToProcess = MaxBatchSize;
                }

                // Avoid the array checking overhead by using pointer indexing!
                //通过使用指针索引避免数组检查开销！
                fixed (VertexPositionColorTexture* vertexArrayFixedPtr = _vertexArray)
                {
                    var vertexArrayPtr = vertexArrayFixedPtr;
                    //Draw the batches
                    for(int i = 0; i < numBatchesToProcess; i++, batchIndex++, index += 4, vertexArrayPtr += 4)
                    {
                        SpriteBatchItem batchItem = _batchItemList[batchIndex];
                        //if the texture changed,we need to flush and bind the new texture
                        var shouldFlush = !ReferenceEquals(batchItem.Texture, tex);
                        if (shouldFlush)
                        {
                            FlushVertexArray(startIndex, index, effect, tex);
                            tex = batchItem.Texture;
                            startIndex = index = 0;
                            vertexArrayPtr = vertexArrayFixedPtr;
                            _device.Textures[0] = tex;
                        }

                        //store the SpriteBatchItem data in out vertexArray
                        //将SpriteBatchItem 的数据存储到顶点数组
                        *(vertexArrayPtr + 0) = batchItem.vertexTL;
                        *(vertexArrayPtr + 1) = batchItem.vertexTR;
                        *(vertexArrayPtr + 2) = batchItem.vertexBL;
                        *(vertexArrayPtr + 3) = batchItem.vertexBR;

                        //Release the texture
                        batchItem.Texture = null;
                    }
                }
                //flush the remaining vertexArray data
                FlushVertexArray(startIndex, index, effect, tex);

                //Update our batch count to continue the process of culling down large batches
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
                _device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                    _vertexArray, 
                    0,
                    vertexCount,
                    _index,
                    0,
                    (vertexCount / 4) * 2,
                    VertexPositionColorTexture.VertexDeclaration);
            }
        }

    }
}