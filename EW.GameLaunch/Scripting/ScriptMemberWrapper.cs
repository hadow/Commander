using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Eluant;
namespace EW.Scripting
{
    /// <summary>
    /// 脚本成员变量封装
    /// </summary>
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
                    throw new LuaException("Unable to Convert {0} to CLR type {1}".F(value.WrappedClrType().Name,propertyInfo.PropertyType));

                propertyInfo.SetValue(Target, clrValue, null);
                
            }
            else
            {
                throw new LuaException("The property '{0}' is read-only".F(MemberInfo.Name));
            }
        }


        LuaValue Invoke(LuaVararg args)
        {
            object[] clrArgs = null;
            try
            {
                if (!IsMethod)
                    throw new LuaException("Trying to invoke a ScriptMemberWrapper that isn't a method !");

                var methodInfo = (MethodInfo)MemberInfo;
                var parameterInfos = methodInfo.GetParameters();

                clrArgs = new object[parameterInfos.Length];

                var argCount = args.Count;
                for(var i = 0; i < parameterInfos.Length; i++)
                {
                    if (i >= argCount)
                    {
                        if (!parameterInfos[i].IsOptional)
                            throw new LuaException("Argument '{0}' of '{1}' is not optional.".F(parameterInfos[i].LuaDocString(), MemberInfo.LuaDocString()));

                        clrArgs[i] = parameterInfos[i].DefaultValue;
                        continue;
                    }

                    if (!args[i].TryGetClrValue(parameterInfos[i].ParameterType, out clrArgs[i]))
                        throw new LuaException("Unable to convert parameter {0} to {1}".F(i, parameterInfos[i].ParameterType.Name));
                }
                return methodInfo.Invoke(Target, clrArgs).ToLuaValue(context);
            }
            finally
            {
                //clean up all the lua arguments that were given to us
                foreach (var arg in args)
                    arg.Dispose();
                args.Dispose();

                if(clrArgs != null)
                {
                    foreach(var arg in clrArgs)
                    {
                        if (!(arg is LuaValue[]))
                            continue;
                        foreach (var value in (LuaValue[])arg)
                            value.Dispose();
                    }
                }
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