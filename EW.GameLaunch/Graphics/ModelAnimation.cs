using System;
using System.Collections.Generic;
using System.Drawing;
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


        public Rectangle ScreenBounds(WPos pos,WorldRenderer wr,float scale)
        {
            var r = Model.AggregateBounds;
            var offset = OffsetFunc != null ? OffsetFunc() : WVec.Zero;

            var xy = wr.ScreenPxPosition(pos) + wr.ScreenPxOffset(offset);

            return Rectangle.FromLTRB(
                xy.X + (int)(r.Left * scale),
                xy.Y + (int)(r.Top * scale),
                xy.X + (int)(r.Right * scale),
                xy.Y + (int)(r.Bottom * scale)
                );
        }


        public bool IsVisible
        {
            get
            {
                return DisableFunc == null || !DisableFunc();
            }
        }
    }
}