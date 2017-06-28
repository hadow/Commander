using System;
using System.Collections.Generic;

namespace EW.Graphics
{

    public struct VoxelRenderData
    {
        public readonly int Start;
        public readonly int Count;
        public readonly Sheet Sheet;

        public VoxelRenderData(int start,int count,Sheet sheet)
        {
            Start = start;
            Count = count;
            Sheet = sheet;
        }
    }


    class VoxelLoader
    {
    }
}