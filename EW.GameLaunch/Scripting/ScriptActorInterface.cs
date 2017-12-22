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


        /// <summary>
        /// ≥ı ºªØ∞Û∂®
        /// </summary>
        void InitializeBindings()
        {
            var commandClasses = Context.ActorCommands[actor.Info].AsEnumerable();


            if (actor.Disposed)
                commandClasses = commandClasses.Where(c => c.HasAttribute<ExposedForDestroyedActors>());

            var args = new object[] { Context, actor };
            var objects = commandClasses.Select(cg => {

                var groupCtor = cg.GetConstructor(new Type[] { typeof(ScriptContext), typeof(Actor) });
                return groupCtor.Invoke(args);
            });

            Bind(objects);
        }

        protected override string DuplicateKeyError(string memberName)
        {

            return "Actor '{0}' defines the command '{1}' on multiple traits".F(actor.Info.Name,memberName);
        }

        protected override string MemberNotFoundError(string memberName)
        {
            var actorName = actor.Info.Name;
            if (actor.IsDead)
                actorName += " (dead)";

            return "Actor '{0}' does not define a property '{1}".F(actorName, memberName);
        }


        public void OnActorDestroyed()
        {
            InitializeBindings();
        }
    }
}