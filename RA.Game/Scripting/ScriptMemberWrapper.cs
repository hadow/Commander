using System;
using System.Collections.Generic;
using System.Reflection;
using Eluant;
namespace RA.Game.Scripting
{
    public class ScriptMemberWrapper
    {
        readonly ScriptContext context;

        public readonly object Target;
        public readonly MemberInfo Member;

        public readonly bool IsMethod;
        public readonly bool IsGetProperty;
        public readonly bool IsSetProperty;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="runtime"></param>
        /// <returns></returns>
        public LuaValue Get(LuaRuntime runtime)
        {
            if (IsMethod)
                return runtime.CreateFunctionFromDelegate((Func<LuaVararg,LuaValue>)Invoke);

            if (IsGetProperty)
                return ((PropertyInfo)Member).GetValue(Target, null).ToLuaValue(context);

            throw new LuaException("the property '{0}' is write-only");
        }


        LuaValue Invoke(LuaVararg args)
        {
            object[] clrArgs = null;
            try
            {
                var methodInfo = (MethodInfo)Member;
                var parameterInfos = methodInfo.GetParameters();

                clrArgs = new object[parameterInfos.Length];

                return methodInfo.Invoke(Target, clrArgs).ToLuaValue(context);
            }
            finally
            {

            }
        }

    }
}