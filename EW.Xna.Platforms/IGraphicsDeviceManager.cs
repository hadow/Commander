using System;

namespace EW.Xna.Platforms
{
    public interface IGraphicsDeviceManager
    {

        bool BeginDraw();

        void CreateDevice();


        void EndDraw();
    }
}