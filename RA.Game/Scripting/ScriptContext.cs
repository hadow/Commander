using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using RA.Game.Graphics;
using Eluant;
namespace RA.Game.Scripting
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ScriptContext:IDisposable
    {

        const int MaxUserScriptMemory = 50 * 1024 * 1024;//脚本最大内存占用量
        const int MaxUserScriptInstructions = 1000000;//脚本指令数量
        public World World { get; private set; }
        public WorldRenderer WorldRenderer { get; private set;}

        readonly MemoryConstrainedLuaRuntime runtime;
        readonly LuaFunction tick;
        readonly Type[] knownActorCommands;
        public readonly Type[] PlayerCommands;

        public ScriptContext(World world,WorldRenderer worldRenderer,IEnumerable<string> scripts)
        {
            runtime = new MemoryConstrainedLuaRuntime();
            this.World = world;
            this.WorldRenderer = worldRenderer;

            runtime.Globals["GameDir"] = "";
            tick = (LuaFunction)runtime.Globals["Tick"];

            //Register globals
            using(var fatalError = runtime.CreateFunctionFromDelegate((Action<string>)FatalError))
            {
                runtime.Globals["FatalError"] = fatalError;
            }
            runtime.Globals["MaxUserScriptInstructions"] = MaxUserScriptInstructions;

            using (var registerGlobal = (LuaFunction)runtime.Globals["RegisterSandboxedGlobal"])
            {
                using(var log = runtime.CreateFunctionFromDelegate((Action<string>)LogDebugMessage))
                {
                    registerGlobal.Call("print", log).Dispose();
                }

                //Register global tables
                var bindings = WarGame.ModData.ObjectCreator.GetTypesImplementing<ScriptGlobal>();
                foreach(var bind in bindings)
                {
                    var ctor = bind.GetConstructors(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(c => {

                        var p = c.GetParameters();
                        return p.Length == 1 && p.First().ParameterType == typeof(ScriptContext);
                    });


                    if (ctor == null)
                        throw new InvalidOperationException("{0} must define a constructor that takes a ScriptContext context parameter");

                    var binding = (ScriptGlobal)ctor.Invoke(new[] { this });
                    using (var obj = binding.ToLuaValue(this))
                        registerGlobal.Call(binding.Name, obj).Dispose();

                }
            }


            runtime.MaxMemoryUse = runtime.MemoryUse + MaxUserScriptMemory;
            //using(var loadScript = (LuaFunction)runtime.Globals["ExecuteSandboxedScript"])
            //{
            //    foreach(var s in scripts)
            //    {
            //        loadScript.Call(s,world.)
            //    }
            //}
        }

        public void Tick(Actor actor)
        {
           
           tick.Call().Dispose();
        }

        public void WorldLoaded()
        {

        }


        /// <summary>
        /// 致命错误
        /// </summary>
        /// <param name="message"></param>
        public void FatalError(string message)
        {

        }

        private void LogDebugMessage(string message)
        {

        }



        public void Dispose() { }
    }
}