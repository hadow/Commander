using System;
using System.Linq;

namespace EW.Scripting
{
    /// <summary>
    /// 
    /// </summary>
    public class ScriptActorInterface:ScriptObjectWrapper
    {
        readonly Actor actor;
        public ScriptActorInterface(ScriptContext context,Actor actor) : base(context)
        {
            this.actor = actor;
            InitializeBindings();
        }


        void InitializeBindings()
        {
            var commandClasses = Context.ActorCommands[actor.Info].AsEnumerable();

            var args = new object[] { Context, actor };
            var objects = commandClasses.Select(cg => {

                var groupCtor = cg.GetConstructor(new Type[] { typeof(ScriptContext), typeof(Actor) });
                return groupCtor.Invoke(args);
            });

            Bind(objects);
        }

        protected override string DuplicateKeyError(string memberName)
        {
            throw new NotImplementedException();
        }

        protected override string MemberNotFoundError(string memberName)
        {
            throw new NotImplementedException();
        }


        public void OnActorDestroyed()
        {
            InitializeBindings();
        }
    }
}