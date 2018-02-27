using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EW.Primitives;
namespace EW.NetWork
{
    class SyncReport
    {
        class Report
        {

            public int Frame;
            public int SyncedRandom;
            public int TotalCount;
            public List<TraitReport> Traits = new List<TraitReport>();
            public List<EffectReport> Effects = new List<EffectReport>();
        }

        struct TraitReport{
            public uint ActorID;
            public string Type;
            public string Owner;
            public string Trait;
            public int Hash;
            public Pair<string[], Values> NamesValues;

        }

        struct EffectReport
        {
            public string Name;
            public int Hash;
            public Pair<string[], Values> NamesValues;
        }

        struct Values{

            static readonly object Sentinel = new object();

            object item1OrArray;
            object item2OrSentinel;
            object item3;
            object item4;

            public Values(int size){

                item1OrArray = null;
                item2OrSentinel = null;
                item3 = null;
                item4 = null;

                if(size>4){
                    item1OrArray = new object[size];
                    item2OrSentinel = Sentinel;
                }
            }

            public object this[int index]{

                get{
                    if (item2OrSentinel == Sentinel)
                        return ((object[])item1OrArray)[index];

                    switch(index){
                        case 0:return item1OrArray;
                        case 1:return item2OrSentinel;
                        case 2: return item3;
                        case 3:return item4;
                        default:
                            throw new ArgumentOutOfRangeException("index");
                    }
                }
                set{
                    
                }
            }

        }


        struct TypeInfo{

            static readonly ParameterExpression SyncParam = Expression.Parameter(typeof(ISync), "synce");
            static readonly ConstantExpression NullString = Expression.Constant(null, typeof(string));
            static readonly ConstantExpression TrueString = Expression.Constant(bool.TrueString, typeof(string));
            static readonly ConstantExpression FalseString = Expression.Constant(bool.FalseString, typeof(string));

            public readonly Func<ISync, object>[] SerializableCopyOfMemberFunctions;

            public readonly string[] Names;

            public TypeInfo(Type type){

                const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                var fields = type.GetFields(Flags).Where(fi => !fi.IsLiteral && !fi.IsStatic && fi.HasAttribute<SyncAttribute>());
                var properties = type.GetProperties(Flags).Where(pi => pi.HasAttribute<SyncAttribute>());

                foreach(var prop in properties){
                    if (!prop.CanRead || prop.GetIndexParameters().Any())
                        throw new InvalidOperationException("Properites using the Sync attribute must be readable and must not use index parameters.\n " +
                                                            "Invalid Property :" + prop.DeclaringType.FullName + "." + prop.Name);
                    
                }

                var sync = Expression.Convert(SyncParam, type);
                SerializableCopyOfMemberFunctions = fields.Select(fi => SerializableCopyOfMember(Expression.Field(sync, fi), fi.FieldType, fi.Name))
                                                          .Concat(properties.Select(pi => SerializableCopyOfMember(Expression.Property(sync, pi), pi.PropertyType, pi.Name))).ToArray();

                Names = fields.Select(fi => fi.Name).Concat(properties.Select(pi => pi.Name)).ToArray();
            }

            static Func<ISync, object> SerializableCopyOfMember(MemberExpression getMember, Type memberType, string name)
            {

                if (memberType.IsValueType)
                {

                    if (memberType == typeof(bool))
                    {

                        var getBoolString = Expression.Condition(getMember, TrueString, FalseString);
                        return Expression.Lambda<Func<ISync, string>>(getBoolString, name, new[] { SyncParam }).Compile();

                    }

                    var boxedCopy = Expression.Convert(getMember, typeof(object));
                    return Expression.Lambda<Func<ISync, object>>(boxedCopy, name, new[] { SyncParam }).Compile();
                }

                return MemberToString(getMember, memberType, name);
            }

            static Func<ISync,string> MemberToString(MemberExpression getMember,Type memberType,string name){

                var toString = memberType.GetMethod("ToString", Type.EmptyTypes);
                Expression getString;
                if(memberType.IsValueType)
                {
                    getString = Expression.Call(getMember, toString);
                }
                else{

                    var memberVariable = Expression.Variable(memberType, getMember.Member.Name);
                    var assignMemberVariable = Expression.Assign(memberVariable, getMember);
                    var member = Expression.Block(new[] { memberVariable }, assignMemberVariable);
                    getString = Expression.Call(member, toString);
                    var nullMember = Expression.Constant(null, memberType);
                    getString = Expression.Condition(Expression.Equal(member, nullMember), NullString, getString);

                }
                return Expression.Lambda<Func<ISync, string>>(getString, name, new[] { SyncParam }).Compile();
            }

        }


        const int NumSyncReports = 5;

        readonly OrderManager orderManager;
        
        static Cache<Type, TypeInfo> typeInfoCache = new Cache<Type, TypeInfo>(t=>new TypeInfo(t));

        readonly Report[] syncReports = new Report[NumSyncReports];
        public SyncReport(OrderManager orderManager)
        {

            this.orderManager = orderManager;
            for(var i = 0; i < NumSyncReports; i++)
            {
                syncReports[i] = new Report();
            }
            
        }


        internal void DumpSyncReport(int frame,IEnumerable<FrameData.ClientOrder> orders)
        {

        }


    }
}