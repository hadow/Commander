using System;
using System.Linq;
using Eluant;
using EW.Scripting;
using EW.Primitives;
using EW.Mods.Common.Traits;
namespace EW.Common.Scripting.Global
{
    [ScriptGlobal("Actor")]
    public class ActorGlobal:ScriptGlobal
    {
        public ActorGlobal(ScriptContext context) : base(context) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="addToWorld"></param>
        /// <param name="initTable"></param>
        /// <returns></returns>
        public Actor Create(string type,bool addToWorld,LuaTable initTable)
        {
            var initDict = new TypeDictionary();
            if (initTable == null)
                Console.WriteLine("init Table is null");
            foreach (var kv in initTable)
            {
                
                using (kv.Key)
                using (kv.Value)
                {
                    //Find the requested type
                    var typeName = kv.Key.ToString();
                    var initType = WarGame.ModData.ObjectCreator.FindType(typeName + "Init");
                    if (initType == null)
                        throw new LuaException("Unknown initializer type '{0}'".F(typeName));

                    //Cast it up to an IactorInit<T>
                    var genericType = initType.GetInterfaces().First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IActorInit));
                    var innerType = genericType.GetGenericArguments().First();
                    var valueType = innerType.IsEnum ? typeof(int) : initType;

                    object value;
                    if (!kv.Value.TryGetClrValue(valueType, out value))
                        throw new LuaException("Invalid data type for '{0}' (expected '{1}')".F(typeName, valueType.Name));

                    var test = initType.GetConstructor(new[] { innerType }).Invoke(new[] { value });
                    initDict.Add(test);
                }
            }

            //The actor must be added to the world at the end of the tick;
            var a = Context.World.CreateActor(false, type, initDict);
            if (addToWorld)
                Context.World.AddFrameEndTask(w => w.Add(a));

            return a;
        }

        public int BuildTime(string type,string queue = null)
        {
            ActorInfo ai;
            if (!Context.World.Map.Rules.Actors.TryGetValue(type, out ai))
                throw new LuaException("Unknown actor type '{0}'".F(type));

            var bi = ai.TraitInfoOrDefault<BuildableInfo>();
            if (bi == null)
                return 0;
            var time = bi.BuildDuration;
            if(time == -1)
            {

            }
            return time;
        }
    }
}