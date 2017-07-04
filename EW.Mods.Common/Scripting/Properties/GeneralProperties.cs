using System;
using EW.Scripting;

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

        public GeneralProperties(ScriptContext context,Actor self) : base(context, self)
        {

        }



    }
}