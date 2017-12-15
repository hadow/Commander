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

        /// <summary>
        /// Specifies whether the actor is in the world.
        /// </summary>
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

        /// <summary>
        /// Specifies whether the actor is alive or dead.
        /// </summary>
        public bool IsDead { get { return Self.IsDead; } }

        /// <summary>
        /// Specifies whether the actor is idle (not performing any activities).
        /// </summary>
        public bool IsIdle { get { return Self.IsIdle; } }
        /// <summary>
        /// The player that owns the actor;
        /// </summary>
        public Player Owner
        {
            get { return Self.Owner; }
            set
            {
                if (Self.Owner != value)
                    Self.ChangeOwner(value);
                    
            }
        }
        /// <summary>
        /// The type of the actor.
        /// </summary>
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

        /// <summary>
        /// The direction that the actor is facing.
        /// </summary>
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
            //Console.WriteLine("Teleport");
            Self.QueueActivity(new SimpleTeleport(cell));
        }


        /// <summary>
        /// Run an arbitrary Lua function.
        /// </summary>
        /// <param name="func"></param>
        [ScriptActorPropertyActivity]
        public void CallFunc(LuaFunction func)
        {
            //Console.WriteLine("CallFunc");
            Self.QueueActivity(new CallLuaFunc(func, Context));
        }

        public void Stop()
        {
            Self.CancelActivity();
        }


        public string Stance
        {
            get
            {
                if (autotarget == null)
                    return null;
                return autotarget.Stance.ToString();
            }
            set
            {
                if (autotarget == null)
                    return;
                UnitStance stance;

                if (!Enum<UnitStance>.TryParse(value, true, out stance))
                    throw new LuaException("Unknown stance type '{0}'".F(value));

                autotarget.SetStance(Self, stance);
            }
        }
    }
}