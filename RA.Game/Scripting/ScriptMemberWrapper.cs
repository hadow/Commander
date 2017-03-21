using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Eluant;
namespace RA.Scripting
{
    public class ScriptMemberWrapper
    {
        readonly ScriptContext context;

        public readonly object Target;
        public readonly MemberInfo MemberInfo;

        public readonly bool IsMethod;
        public readonly bool IsGetProperty;
        public readonly bool IsSetProperty;


        public ScriptMemberWrapper(ScriptContext context,object target,MemberInfo memberInfo)
        {
            this.context = context;
            Target = target;
            MemberInfo = memberInfo;

            var property = memberInfo as PropertyInfo;

            if(property != null)
            {
                IsGetProperty = property.GetGetMethod() != null;
                IsSetProperty = property.GetSetMethod() != null;
            }
            else
            {
                IsMethod = true;
            }
        }

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
                return ((PropertyInfo)MemberInfo).GetValue(Target, null).ToLuaValue(context);

            throw new LuaException("the property '{0}' is write-only");
        }

        public void Set(LuaRuntime runtime,LuaValue value)
        {
            if(IsSetProperty)
            {
                var propertyInfo = (PropertyInfo)MemberInfo;
                object clrValue;
                if (!value.TryGetClrValue(propertyInfo.PropertyType, out clrValue))
                    throw new LuaException("Unable to Convert {0} to CLR type {1}");

                propertyInfo.SetValue(Target, clrValue, null);
                
            }
        }

        LuaValue Invoke(LuaVararg args)
        {
            object[] clrArgs = null;
            try
            {
                var methodInfo = (MethodInfo)MemberInfo;
                var parameterInfos = methodInfo.GetParameters();

                clrArgs = new object[parameterInfos.Length];

                return methodInfo.Invoke(Target, clrArgs).ToLuaValue(context);
            }
            finally
            {

            }
        }


        public static IEnumerable<MemberInfo> WrappableMembers(Type t)
        {
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;

            return t.GetMembers(flags).Where(mi=> {

                if (mi is PropertyInfo)
                    return true;

                var method = mi as MethodInfo;
                if (method != null && !method.IsGenericMethodDefinition && !method.IsSpecialName)
                    return true;

                return false;
            });
        }

    }
}