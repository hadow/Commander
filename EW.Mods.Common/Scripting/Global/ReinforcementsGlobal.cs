using System;
using System.Collections.Generic;
using Eluant;
using EW.Activities;
using EW.Effects;
using EW.Mods.Common.Traits;
using EW.Primitives;
using EW.Scripting;
using EW.Traits;
namespace EW.Mods.Common.Scripting
{
    [ScriptGlobal("Reinforcements")]
    public class ReinforcementsGlobal:ScriptGlobal
    {

        readonly DomainIndex domainIndex;

        public ReinforcementsGlobal(ScriptContext context) : base(context)
        {
            domainIndex = context.World.WorldActor.Trait<DomainIndex>();
        }


        Actor CreateActor(Player owner,string actorType,bool addToWorld,CPos? entryLocation = null,CPos? nextLocation = null)
        {
            ActorInfo ai;
            if (!Context.World.Map.Rules.Actors.TryGetValue(actorType, out ai))
                throw new LuaException("Unknown actor type '{0}'".F(actorType));

            var initDict = new TypeDictionary();

            initDict.Add(new OwnerInit(owner));

            if (entryLocation.HasValue)
            {
                var pi = ai.TraitInfoOrDefault<AircraftInfo>();
                initDict.Add(new CenterPositionInit(owner.World.Map.CenterOfCell(entryLocation.Value) + new WVec(0, 0, pi != null ? pi.CruiseAltitude.Length : 0)));
                initDict.Add(new LocationInit(entryLocation.Value));

            }

            if(entryLocation.HasValue && nextLocation.HasValue)
            {
                initDict.Add(new FacingInit(Context.World.Map.FacingBetween(CPos.Zero, CPos.Zero + (nextLocation.Value - entryLocation.Value), 0)));
            }

            var actor = Context.World.CreateActor(addToWorld, actorType, initDict);
            return actor;
        }


        /// <summary>
        /// 增援
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="actorTypes"></param>
        /// <param name="entryPath"></param>
        /// <param name="interval"></param>
        /// <param name="actionFunc"></param>
        /// <returns></returns>
        public Actor[] Reinforce(Player owner,string[] actorTypes,CPos[] entryPath,int interval = 25,LuaFunction actionFunc = null)
        {
            var actors = new List<Actor>();
            for(var i = 0; i < actorTypes.Length; i++)
            {
                var af = actionFunc != null ? (LuaFunction)actionFunc.CopyReference() : null;

                var actor = CreateActor(owner, actorTypes[i], false, entryPath[0], entryPath.Length > 1 ? entryPath[1] : (CPos?)null);

                actors.Add(actor);
                var actionDelay = i * interval;

                Action actorAction = () =>
                {
                    Context.World.Add(actor);
                    for (var j = 1; j < entryPath.Length; j++)
                        Move(actor, entryPath[j]);


                    if (af != null)
                    {
                        actor.QueueActivity(new CallFunc(()=>
                        {
                            using (af)
                            using (var a = actor.ToLuaValue(Context))
                                af.Call(a);
                        }));
                    }
                };

                Context.World.AddFrameEndTask(w => w.Add(new DelayedAction(actionDelay, actorAction)));
            }

            return actors.ToArray();
        }


        void Move(Actor actor,CPos dest)
        {
            var move = actor.TraitOrDefault<IMove>();
            if (move == null)
                return;
            actor.QueueActivity(move.MoveTo(dest, 2));
        }
    }
}