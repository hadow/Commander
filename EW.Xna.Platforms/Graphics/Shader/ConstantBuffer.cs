using System;


namespace EW.Xna.Platforms.Graphics
{
    /// <summary>
    /// 常量缓冲区
    /// 将着色器常量数据提供给pipeline
    /// </summary>
    internal partial class ConstantBuffer:GraphicsResource
    {

        private readonly byte[] _buffer;

        private readonly int[] _parameters;

        private readonly int[] _offsets;

        private readonly string _name;

        private ulong _stateKey;

        private bool _dirty;

        public ConstantBuffer(GraphicsDevice device,int sizeInBytes,int[] parameterIndexes,int[] parameterOffsets,string name)
        {
            GraphicsDevice = device;
            _buffer = new byte[sizeInBytes];
            _parameters = parameterIndexes;
            _offsets = parameterOffsets;
            _name = name;
            PlatformInitialize();
        }

        public ConstantBuffer(ConstantBuffer cloneSource)
        {
            GraphicsDevice = cloneSource.GraphicsDevice;
            
            //Share the immutable types.
            _name = cloneSource._name;
            _parameters = cloneSource._parameters;
            _offsets = cloneSource._offsets;

            //Clone the mutable types.
            _buffer = (byte[])cloneSource._buffer.Clone();
            PlatformInitialize();
        }


        internal void Clear()
        {
            PlatformClear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        public void Update(EffectParameterCollection parameters)
        {
            if (_stateKey > EffectParameter.NextStateKey)
                _stateKey = 0;

            for(var p = 0; p < _parameters.Length; p++)
            {
                var index = _parameters[p];
                var param = parameters[index];
                if (param.StateKey < _stateKey)
                    continue;

                var offset = _offsets[p];
                _dirty = true;

                SetParameter(offset, param);
                
            }

            _stateKey = EffectParameter.NextStateKey;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private int SetParameter(int offset,EffectParameter param)
        {
            const int elementSize = 4;
            const int rowSize = elementSize * 4;
            var rowsUsed = 0;

            var elements = param.Elements;
            if (elements.Count > 0)
            {
                for(var i = 0; i < elements.Count; i++)
                {
                    var rowUsedSubParam = SetParameter(offset, elements[i]);
                    offset += rowUsedSubParam * rowSize;
                    rowsUsed += rowUsedSubParam;
                }
            }
            else if(param.Data != null)
            {
                switch (param.ParameterType)
                {
                    case EffectParameterType.Single:
                    case EffectParameterType.Int32:
                    case EffectParameterType.Bool:
                        if(param.ParameterClass == EffectParameterClass.Matrix)
                        {
                            rowsUsed = param.ColumnCount;
                            SetData(offset, param.ColumnCount, param.RowCount, param.Data);
                        }
                        else
                        {
                            rowsUsed = param.RowCount;
                            SetData(offset, param.RowCount, param.ColumnCount, param.Data);
                        }
                        break;
                    default:
                        throw new NotSupportedException("Not Supported !");
                }
            }
            return rowsUsed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        /// <param name="data"></param>
        private void SetData(int offset,int rows,int columns,object data)
        {
            const int elementSize = 4;
            const int rowSize = elementSize * 4;

            if (rows == 1 && columns == 1)
            {
                //EffectParameter stores all values in array by default.
                if (data is Array)
                    Buffer.BlockCopy(data as Array, 0, _buffer, offset, elementSize);
                else
                    throw new NotImplementedException();
            }
            else if (rows == 1 || (rows == 4 && columns == 4))
            {
                Buffer.BlockCopy(data as Array, 0, _buffer, offset, rows * columns * elementSize);
            }
            else
            {
                var source = data as Array;

                var stride = (columns * elementSize);
                for (var y = 0; y < rows; y++)
                    Buffer.BlockCopy(source, stride*y, _buffer, offset + (rowSize * y), columns * elementSize);
            }
        }
        
    }
}