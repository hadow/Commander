using System;
using System.Collections.Generic;

namespace RA.Mobile.Platforms.Content
{
    public partial class ContentManager:IDisposable
    {

        private string _rootDirectory = string.Empty;

        private static List<WeakReference> ContentManagers = new List<WeakReference>();
        /// <summary>
        /// ÖØÔØÍ¼ĞÎÄÚÈİ
        /// </summary>
        internal static void ReloadGraphicsContent()
        {

        }

        public void Dispose() { }



    }
}