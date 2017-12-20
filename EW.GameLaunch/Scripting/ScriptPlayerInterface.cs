using System;
using System.Linq;
namespace EW.Scripting
{
    public class ScriptPlayerInterface:ScriptObjectWrapper
    {
        readonly Player player;

        protected override string DuplicateKeyError(string memberName)
        {
            throw new NotImplementedException();
        }

        protected override string MemberNotFoundError(string memberName)
        {
            throw new NotImplementedException();
        }


        public ScriptPlayerInterface(ScriptContext context,Player player):base(context)
        {
            this.player = player;

            var args = new object[] { context, player };
            var objects = context.PlayerCommands.Select(cg =>
            {
                var groupCtor = cg.GetConstructor(new Type[] { typeof(ScriptContext), typeof(Player) });
                return groupCtor.Invoke(args);
            });

            Bind(objects);
        }

    }
}