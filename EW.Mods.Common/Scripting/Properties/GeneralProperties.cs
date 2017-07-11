using System;
using EW.Scripting;
using EW.Traits;
using EW.Mods.Common.Traits;
using EW.Mods.Common.Activities;
using Eluant;
namespace EW.Mods.Common.Scripting
{
    [ExposedForDestroyedActors]
    [ScriptPropertyGroup("General")]
    public class BaseActorProperties : ScriptActorProperties
    {
        public BaseActorProperties(ScriptContext context,Actor self) : base(context, self) { }

        public bool IsInWorld
        {
            get { return Self.IsInWorld; }
            set
            {
                if (value)
                    Self.World.AddFrameEndTask(w => w.Add(Self));
                else
                    Self.World.AddFrameEndTask(w => w.Remove(Self));
            }
        }

        public bool IsDead { get { return Self.IsDead; } }

        public bool IsIdle { get { return Self.IsIdle; } }

        public Player Owner
        {
            get { return Self.Owner; }
            set
            {
                if (Self.Owner != value)
                    Self.ChangeOwner(value);
                    
            }
        }

        public string Type { get { return Self.Info.Name; } }

        public bool HasProperty(string name)
        {
            return Self.HasScriptProperty(name);
        }
    }


    [ScriptPropertyGroup("General")]
    public class GeneralProperties:ScriptActorProperties
    {
        readonly IFacing facing;

        readonly AutoTarget autotarget;

        readonly ScriptTags scriptTags;


        public GeneralProperties(ScriptContext context,Actor self) : base(context, self)
        {

            facing = self.TraitOrDefault<IFacing>();
            autotarget = self.TraitOrDefault<AutoTarget>();
            scriptTags = self.TraitOrDefault<ScriptTags>();

        }


        /// <summary>
        /// The actor position in cell coordinates.
        /// </summary>
        public CPos Location { get { return Self.Location; } }

        /// <summary>
        /// The actor position in world coordinates.
        /// </summary>
        public WPos CenterPosition { get { return Self.CenterPosition; } }

        public int Facing
        {
            get
            {
                if (facing == null)
                    throw new LuaException("Actor '{0}' doesn't define a facing".F(Self));
                return facing.Facing;
            }
        }

        /// <summary>
        /// Instantly moves the actor to the specified cell.
        /// </summary>
        /// <param name="cell"></param>
        [ScriptActorPropertyActivity]
        public void Teleport(CPos cell)
        {
            Self.QueueActivity(new SimpleTeleport(cell));
        }

        [ScriptActorPropertyActivity]
        public void CallFunc(LuaFunction func)
        {
            Self.QueueActivity(new CallLuaFunc(func, Context));
        }

    }
}