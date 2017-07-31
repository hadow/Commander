using System;
using System.Collections.Generic;
using EW.Primitives;
namespace EW.Graphics
{
    public struct ModelAnimation
    {

        public readonly IModel Model;

        public readonly Func<WVec> OffsetFunc;

        public readonly Func<IEnumerable<WRot>> RotationFunc;

        public readonly Func<bool> DisableFunc;

        public readonly Func<uint> FrameFunc;

        public readonly bool ShowShadow;

        public ModelAnimation(IModel model,Func<WVec> offset,Func<IEnumerable<WRot>> rotation,Func<bool> disable,Func<uint> frame,bool showshadow)
        {
            Model = model;
            OffsetFunc = offset;
            RotationFunc = rotation;
            DisableFunc = disable;
            FrameFunc = frame;
            ShowShadow = showshadow;
        }


    }
}