using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using EW.Support;
namespace EW.Mods.Common.Lint
{
    public class LintExts
    {
        public static IEnumerable<string> GetFieldValues(object ruleInfo,FieldInfo fieldInfo,Action<string> emitError)
        {
            var type = fieldInfo.FieldType;
            if (type == typeof(string))
                return new string[] { (string)fieldInfo.GetValue(ruleInfo) };

            if (typeof(IEnumerable<string>).IsAssignableFrom(type))
                return fieldInfo.GetValue(ruleInfo) as IEnumerable<string>;

            if(type == typeof(BooleanExpression) || type == typeof(IntegerExpression))
            {
                var expr = (VariableExpression)fieldInfo.GetValue(ruleInfo);
                return expr != null ? expr.Variables : Enumerable.Empty<string>();
            }

            throw new InvalidOperationException("Bad type for reference on {0}.{1}.Supported types:string,IEnumerable<string>,BooleanExpression,IntegerExpression".F(ruleInfo.GetType().Name, fieldInfo.Name));
        }

        public static IEnumerable<string> GetPropertyValues(object ruleInfo,PropertyInfo propertyInfo,Action<string> emitError)
        {
            var type = propertyInfo.PropertyType;
            if(type == typeof(string))
                return new []{ (string)propertyInfo.GetValue(ruleInfo)};

            if (typeof(IEnumerable).IsAssignableFrom(type))
                return (IEnumerable<string>)propertyInfo.GetValue(ruleInfo);

            if(type == typeof(BooleanExpression) || type == typeof(IntegerExpression))
            {
                var expr = (VariableExpression)propertyInfo.GetValue(ruleInfo);
                return expr != null ? expr.Variables : Enumerable.Empty<string>();
            }

            throw new InvalidOperationException("Bad type for reference on {0}.{1}.Supported types:string,IEnumerable<string>,BooleanExpression,IntegerExpression".F(ruleInfo.GetType().Name, propertyInfo.Name));

        }


    }
}