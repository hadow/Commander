using System;
using System.Collections.Generic;
using RA.Game.Graphics;
using Eluant;
namespace RA.Game
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
                
            }
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