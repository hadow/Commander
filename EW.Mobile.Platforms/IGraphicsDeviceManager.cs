using System;

namespace EW.Mobile.Platforms
{
    public interface IGraphicsDeviceManager
    {

        bool BeginDraw();

        void CreateDevice();


        void EndDraw();
    }
}