using System;
using System.Linq;
using Eluant;
using EW.Scripting;
using EW.Primitives;
using EW.Mods.Common.Traits;
namespace EW.Mods.Common.Scripting
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
            //Convert table entries into ActorInits
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
                    var genericType = initType.GetInterfaces().First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IActorInit<>));
                    var innerType = genericType.GetGenericArguments().First();
                    var valueType = innerType.IsEnum ? typeof(int) : innerType;

                    //Try and coerce the table value to the required type
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

        /// <summary>
        /// Builds the time.
        /// </summary>
        /// <returns>The time.</returns>
        /// <param name="type">Type.</param>
        /// <param name="queue">Queue.</param>
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
                var valued = ai.TraitInfoOrDefault<ValuedInfo>();
                if (valued == null)
                    return 0;
                else
                    time = valued.Cost;


            }

            int pbi;
            if(queue != null){

                var pqueue = Context.World.Map.Rules.Actors.Values.SelectMany(a => a.TraitInfos<ProductionQueueInfo>())
                                    .Where(x=>x.Type == queue).FirstOrDefault();
                if (pqueue == null)
                    throw new LuaException("The specified queue '{0}' does not exist".F(queue));
                pbi = pqueue.BuildDurationModifier;


            }
            else{

                var pqueue = Context.World.Map.Rules.Actors.Values.SelectMany(a => a.TraitInfos<ProductionQueueInfo>()
                                                                              .Where(x => bi.Queue.Contains(x.Type))).FirstOrDefault();

                if (pqueue == null)
                    throw new LuaException("No actors can produce actor '{0}' ".F(type));

                pbi = pqueue.BuildDurationModifier;

            }
            return time;
        }
    }
}