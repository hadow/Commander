﻿using System;

namespace EW.Framework
{
    // Required to allow platforms other than iOS use the same code.
    // just don't include this on iOS
    [AttributeUsage (AttributeTargets.Delegate)]
    internal sealed class MonoNativeFunctionWrapper : Attribute
    {
    }
}

