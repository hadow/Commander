using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace EW.Mods.Common.Traits
{

    public class MapCreepsInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new MapCreeps(); }
    }


    public class MapCreeps
    {
    }
}