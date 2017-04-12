using System;
using System.Collections.Generic;
using System.Linq;
using Eluant;
using Eluant.ObjectBinding;
using EW.Scripting;
using EW.Activities;
using EW.Traits;
using EW.Primitives;
using EW.Mobile.Platforms;
namespace EW
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Actor:IScriptBindable,IScriptNotifyBind, ILuaTableBinding,ILuaEqualityBinding,IEquatable<Actor>,IDisposable
    {

        public readonly ActorInfo Info;

        public readonly World World;

        public readonly uint ActorID;

        public IOccupySpace OccupiesSpace { get; private set; }

        public Rectangle Bounds { get; private set; }

        public Rectangle VisualBounds { get; private set; }

        public bool IsInWorld { get; internal set; }
        public bool Disposed { get; private set; }
        Activity currentActivity;


        internal Actor(World world,string name,TypeDictionary initDict)
        {
            var init = new ActorInitializer(this, initDict);
            
            World = world;
            ActorID = world.NextAID();
            
            if(name != null)
            {
                name = name.ToLowerInvariant();

                if (!world.Map.Rules.Actors.ContainsKey(name))
                {
                    throw new NotImplementedException("No rules definition for unit {0}".F(name));
                }
                Info = world.Map.Rules.Actors[name];
                foreach(var trait in Info.TraitsInConstructOrder())
                {
                    AddTrait(trait.Create(init));
                    if(trait is IOccupySapceInfo)
                    {
                        OccupiesSpace = Trait<IOccupySpace>();
                    }
                }
            }

            Bounds = DetermineBounds();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Rectangle DetermineBounds()
        {
            var si = Info.TraitInfoOrDefault<SelectableInfo>();
            var size = (si != null && si.Bounds != null) ? new Vector2(si.Bounds[0], si.Bounds[1]) : TraitsImplementing<IAutoSelectionSize>().Select(x => x.SelectionSize(this)).FirstOrDefault();

            var offset = -size / 2;
            if (si != null && si.Bounds != null && si.Bounds.Length > 2)
                offset += new Vector2(si.Bounds[2], si.Bounds[3]);

            return new Rectangle((int)offset.X, (int)offset.Y, (int)size.X, (int)size.Y);
        }
        

        public void Tick()
        {
            currentActivity = ActivityUtils.RunActivity(this, currentActivity);
        }


        public bool Equals(Actor other)
        {
            return ActorID == other.ActorID;
        }

        public void Dispose()
        {

        }

        #region Trait

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trait"></param>
        public void AddTrait(object trait)
        {
            World.TraitDict.AddTrait(this, trait);
        }
        public T TraitOrDefault<T>()
        {
            return World.TraitDict.GetOrDefault<T>(this);
        }

        public T Trait<T>()
        {
            return World.TraitDict.Get<T>(this);
        }

        public IEnumerable<T> TraitsImplementing<T>()
        {
            return World.TraitDict.WithInterface<T>(this);
        }
        #endregion


        #region Scripting interface


        Lazy<ScriptActorInterface> luaInterface;
        public void OnScriptBind(ScriptContext context)
        {
            if (luaInterface == null)
                luaInterface = Exts.Lazy(() => new ScriptActorInterface(context, this));
        }

        public LuaValue this[LuaRuntime runtime,LuaValue keyValue]
        {
            get { return luaInterface.Value[runtime, keyValue]; }
            set { luaInterface.Value[runtime, keyValue] = value; }
        }

        public LuaValue Equals(LuaRuntime runtime,LuaValue left,LuaValue right)
        {
            Actor a, b;
            if(!left.TryGetClrValue(out a) || !right.TryGetClrValue(out b))
            {
                return false;
            }
            return a == b;
        }

        public LuaValue ToString(LuaRuntime runtime)
        {
            return "Actor ({0})".F(this);
        }

        public bool HasScriptProperty(string name)
        {
            return luaInterface.Value.ContainsKey(name);
        }


        #endregion

    }
}