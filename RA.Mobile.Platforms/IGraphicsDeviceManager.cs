using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace RA.Mobile.Platforms
{
    public interface IGraphicsDeviceManager
    {

        bool BeginDraw();

        void CreateDevice();


        void EndDraw();
    }
}