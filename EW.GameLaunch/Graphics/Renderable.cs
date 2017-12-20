using System;
using EW.OpenGLES;
namespace EW.Graphics
{

    public interface IRenderable
    {
        WPos Pos { get; }

        PaletteReference Palette { get; }

        int ZOffset { get; }

        bool IsDecoration { get; }

        IRenderable WithPalette(PaletteReference newPalette);

        IRenderable WithZOffset(int newOffset);

        IRenderable OffsetBy(WVec offset);

        IRenderable AsDecoration();

        IFinalizedRenderable PrepareRender(WorldRenderer wr);
    }

    public interface IFinalizedRenderable
    {
        void Render(WorldRenderer wr);

        void RenderDebugGeometry(WorldRenderer wr);

        Rectangle ScreenBounds(WorldRenderer wr);
    }
}
