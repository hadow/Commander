﻿using System;
using Eluant;
using EW.Activities;
using EW.Scripting;
namespace EW.Mods.Common.Activities
{
    public sealed class CallLuaFunc:Activity,IDisposable
    {

        readonly ScriptContext context;

        LuaFunction function;

        public CallLuaFunc(LuaFunction function,ScriptContext context)
        {
            this.function = (LuaFunction)function.CopyReference();
            this.context = context;

        }

        public override Activity Tick(Actor self)
        {
            try
            {
                if (function != null)
                    function.Call().Dispose();
            }
            catch(Exception ex)
            {
                context.FatalError(ex.Message);
            }
            Dispose();
            return NextActivity;
        }

        public override void Cancel(Actor self)
        {
            Dispose();
            base.Cancel(self);
        }

        public void Dispose()
        {
            if (function == null)
                return;
            function.Dispose();
            function = null;
        }
    }
}