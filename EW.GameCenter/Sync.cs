using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using EW.Traits;
using EW.Primitives;

namespace EW
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class SyncAttribute : Attribute { }

    public interface ISync { }
    /// <summary>
    /// 
    /// </summary>
    public static class Sync
    {
        static readonly ConcurrentCache<Type, Func<object, int>> HashFunctions = new ConcurrentCache<Type, Func<object, int>>(GenerateHashFunc);

        static readonly Dictionary<Type, MethodInfo> CustomHashFunctions = new Dictionary<Type, MethodInfo>
        {
            {typeof(CPos),((Func<CPos,int>)HashCPos).Method },
            {typeof(CVec),((Func<CVec,int>)HashCVec).Method },
            {typeof(WDist),((Func<WDist,int>)HashUsingHashCode).Method },
            {typeof(WPos),((Func<WPos,int>)HashUsingHashCode).Method },
            { typeof(WVec),((Func<WVec,int>)HashUsingHashCode).Method},
            {typeof(WAngle),((Func<WAngle,int>)HashUsingHashCode).Method },
            {typeof(WRot),((Func<WRot,int>)HashUsingHashCode).Method },
            {typeof(Actor),((Func<Actor,int>)HashActor).Method },
            {typeof(Player),((Func<Player,int>)HashPlayer).Method },
            {typeof(Target),((Func<Target,int>)HashTarget).Method }
        };
        internal static Func<object,int> GetHashFunction(ISync sync)
        {
            return HashFunctions[sync.GetType()];
        }


        static Func<object,int> GenerateHashFunc(Type t)
        {
            var d = new DynamicMethod("hash_{0}".F(t.Name), typeof(int), new Type[] { typeof(object) }, t);
            var il = d.GetILGenerator();
            var this_ = il.DeclareLocal(t).LocalIndex;
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, t);
            il.Emit(OpCodes.Stloc, this_);
            il.Emit(OpCodes.Ldc_I4_0);

            const BindingFlags Binding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            foreach(var field in t.GetFields(Binding).Where(x => x.HasAttribute<SyncAttribute>()))
            {
                il.Emit(OpCodes.Ldloc, this_);
                il.Emit(OpCodes.Ldfld, field);

                EmitSyncOpcodes(field.FieldType, il);
            }

            foreach(var prop in t.GetProperties(Binding).Where(x => x.HasAttribute<SyncAttribute>()))
            {
                il.Emit(OpCodes.Ldloc, this_);
                il.EmitCall(OpCodes.Call, prop.GetGetMethod(), null);

                EmitSyncOpcodes(prop.PropertyType, il);
            }

            il.Emit(OpCodes.Ret);
            return (Func<object,int>)d.CreateDelegate(typeof(Func<object, int>));

        }


        static void EmitSyncOpcodes(Type type,ILGenerator il)
        {
            if (CustomHashFunctions.ContainsKey(type))
                il.EmitCall(OpCodes.Call, CustomHashFunctions[type], null);
            else if (type == typeof(bool))
            {
                var l = il.DefineLabel();
                il.Emit(OpCodes.Ldc_I4, 0xaaa);
                il.Emit(OpCodes.Brtrue, l);
                il.Emit(OpCodes.Pop);
                il.Emit(OpCodes.Ldc_I4, 0x555);
                il.MarkLabel(l);
            }
            else if (type != typeof(int))
                throw new NotImplementedException("SyncAttribute on member of unhashable type:{0}".F(type.FullName));

            il.Emit(OpCodes.Xor);
        }

        public static int HashCPos(CPos i2)
        {
            return ((i2.X * 5) ^ (i2.Y * 3)) / 4;
        }

        public static int HashCVec(CVec i2)
        {
            return ((i2.X * 5) ^ (i2.Y * 3)) / 4;
        }


        public static int HashActor(Actor a)
        {
            if (a != null)
                return (int)(a.ActorID << 16);
            return 0;
        }

        public static int HashPlayer(Player p)
        {
            if (p != null)
                return (int)(p.PlayerActor.ActorID << 16) * 0x567;
            return 0;
        }

        public static int HashUsingHashCode<T>(T t)
        {
            return t.GetHashCode();
        }

        public static int HashTarget(Target t)
        {
            switch (t.Type)
            {
                case TargetT.Actor:
                    return (int)(t.Actor.ActorID << 16) * 0x567;
                case TargetT.FrozenActor:
                    if (t.FrozenActor.Actor == null)
                        return 0;
                    return (int)(t.FrozenActor.Actor.ActorID << 16)*0x567;
                case TargetT.Terrain:
                    return HashUsingHashCode(t.CenterPosition);
                default:
                case TargetT.Invalid:
                    return 0;
            }
        }




    }
}