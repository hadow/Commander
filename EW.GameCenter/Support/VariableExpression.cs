using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Expressions =  System.Linq.Expressions;
using System.IO;
namespace EW.Support
{
    public abstract class VariableExpression
    {
        enum CharClass
        {
            Whitespace,
            Operator,
            Mixed,
            Id,
            Digit
        }

        enum ExpressionT
        {
            Int,
            Bool
        }

        /// <summary>
        /// 结合性
        /// </summary>
        enum Associativity { Left,Right}
        enum Grouping { None,Parens}

        enum Sides
        {
            //Value type
            None =0,


            Left =-1,

            Right =2,

            //Binary+ Operator
            Both = Left | Right,
        }
        /// <summary>
        /// 
        /// </summary>
        enum TokenType
        {
            //fixed values
            False,
            True,

            //operators
            OpenParen,
            CloseParen,
            Not,

            And,
            Or,
            Equals,
            NotEquals,
            LessThan,
            LessThanOrEqual,
            GreaterThan,
            GreaterThanOrEqual,
            Add,
            Subtract,
            Multiply,

            Invalid,
        }

        enum Precedence
        {
            Unary = 16, //一元的
            Multiplication = 12,
            Addition = 11,
            Relation = 9,
            Equality = 8,
            And = 4,
            Or = 3,
            Binary = 0,
            Value =0,
            Parens = -1,//括弧
            Invalid=~0,

        }

        struct TokenTypeInfo
        {
            public readonly string Symbol;

            public readonly Precedence Precedence;

            public readonly Sides OperandSides;
            public readonly Sides WhitespaceSides;

            public readonly Associativity Associativity;

            public readonly Grouping Opens;
            public readonly Grouping Closes;

            public TokenTypeInfo(string symbol,Precedence precedence,Sides operandSides = Sides.None,
                Associativity associativity = Associativity.Left,Grouping opens = Grouping.None,Grouping closes = Grouping.None)
            {
                Symbol = symbol;
                Precedence = precedence;
                OperandSides = operandSides;
                WhitespaceSides = Sides.None;
                Associativity = associativity;

                Opens = opens;
                Closes = closes;
            }



        }
        public readonly string Expression;

        readonly HashSet<string> variables = new HashSet<string>();

        public IEnumerable<string> Variables { get { return variables; } }

        static readonly ParameterExpression SymbolsParam = Expressions.Expression.Parameter(typeof(IReadOnlyDictionary<string, int>), "symbols");
        static readonly ConstantExpression Zero = Expressions.Expression.Constant(0);
        static readonly ConstantExpression One = Expressions.Expression.Constant(1);
        static readonly ConstantExpression False = Expressions.Expression.Constant(false);
        static readonly ConstantExpression True = Expressions.Expression.Constant(true);

        static readonly TokenTypeInfo[] TokenTypeInfos = CreateTokenTypeInfoEnumeration().ToArray();

        public VariableExpression(string expression)
        {
            Expression = expression;
        }

        static IEnumerable<TokenTypeInfo> CreateTokenTypeInfoEnumeration()
        {
            for(var i = 0; i <= (int)TokenType.Invalid; i++)
            {
                switch ((TokenType)i)
                {
                    case TokenType.Invalid:
                        yield return new TokenTypeInfo("(<INVALID>)", Precedence.Invalid);
                        continue;
                    case TokenType.False:
                        yield return new TokenTypeInfo("false", Precedence.Value);
                        continue;
                    case TokenType.True:
                        yield return new TokenTypeInfo("true", Precedence.Value);
                        continue;


                }

                throw new InvalidProgramException("CreateTokenTypeInfoEnumeration is missing a TokenTypeInfo entry for TokenType.{0}".F(Enum<TokenType>.GetValues()[i]));
            }
        }


        static CharClass CharClassOf(char c)
        {
            switch (c)
            {
                case '~':
                case '!':
                case '%':
                case '^':
                    return CharClass.Operator;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return CharClass.Digit;
                case ' ':
                case '\t':
                case '\n':
                case '\r':
                    return CharClass.Whitespace;
                default:
                    return char.IsWhiteSpace(c)?CharClass.Whitespace:CharClass.Id;
            }
        }

        protected Func<IReadOnlyDictionary<string,int>,T> Compile<T>()
        {
            ExpressionT resultType;
            if (typeof(T) == typeof(int))
                resultType = ExpressionT.Int;
            else if (typeof(T) == typeof(bool))
                resultType = ExpressionT.Bool;
            else
                throw new InvalidCastException("Variable expressions can only be int or bool ");

            return Expressions.Expression.Lambda<Func<IReadOnlyDictionary<string, int>, T>>(Build(resultType), SymbolsParam).Compile();
        }

        Expression Build(ExpressionT resultType)
        {
            var tokens = new List<Token>();
            var currentOpeners = new Stack<Token>();
            Token lastToken = null;


            return new Compiler().Build(null, resultType);
        }


        /// <summary>
        /// 
        /// </summary>
        class Token
        {

            public readonly TokenType Type;
            public readonly int Index;
            
            public virtual string Symbol { get { return TokenTypeInfos[(int)Type].Symbol; } }

            public Token(TokenType type,int index)
            {
                Type = type;
                Index = index;
            }

            public static TokenType GetNextType(string expression,ref int i,TokenType lastType = TokenType.Invalid)
            {
                var start = i;

                switch (expression[i])
                {
                    case '!':
                        return TokenType.Not;
                    case '<':
                        return TokenType.LessThan;
                    case '>':
                        return TokenType.GreaterThan;
                    case '=':
                        return TokenType.Equals;
                    case '&':
                        throw new InvalidDataException("Unexpected character '&' at index {0} - should it b '&&' ?".F(start));
                    case '|':
                        throw new InvalidDataException("Unexpected character '|' at index {0} - should it b '||' ?".F(start));
                    case '(':
                        return TokenType.OpenParen;
                    case ')':
                        return TokenType.CloseParen;

                }
            }

            public static Token GetNext(string expression,ref int i,TokenType lastType = TokenType.Invalid)
            {
                if (i == expression.Length)
                    return null;

                var whitespaceBefore = false;
                if (CharClassOf(expression[i]) == CharClass.Whitespace)
                {
                    whitespaceBefore = true;
                    while (CharClassOf(expression[i]) == CharClass.Whitespace)
                    {
                        if (++i == expression.Length)
                            return null;
                    }
                }
                else if (lastType == TokenType.Invalid)
                    whitespaceBefore = true;


                var start = i;

                var type = 
            }
        }

        class Compiler
        {

            readonly AstStack ast = new AstStack();

            public Expression Build(Token[] postfix,ExpressionT resultType)
            {
                return ast.Pop(resultType);
            }
        }
        static Expression IfThenElse(Expression test,Expression ifTrue,Expression ifFalse)
        {
            return Expressions.Expression.Condition(test, ifTrue, ifFalse);
        }
        class AstStack
        {
            readonly List<Expression> expressions = new List<Expressions.Expression>();
            readonly List<ExpressionT> types = new List<ExpressionT>();

            public Expression Peek(ExpressionT toType)
            {
                var fromType = types[types.Count - 1];
                var expression = expressions[expressions.Count - 1];
                if (toType == fromType)
                    return expression;

                throw new InvalidProgramException("Unable to convert ExpressionType.{0} to ExpressionType.{1}".F(Enum<ExpressionT>.GetValues()[(int)fromType], Enum<ExpressionT>.GetValues()[(int)toType]));
            }

            public void Push(Expression expression)
            {
                expressions.Add(expression);

                if (expression.Type == typeof(int))
                    types.Add(ExpressionT.Int);
                else if (expression.Type == typeof(bool))
                    types.Add(ExpressionT.Bool);
                else
                    throw new InvalidOperationException("Unhandled result type {0} for {1}".F(expression.Type, expression));
            }

            public Expression Pop(ExpressionT type)
            {
                var expression = Peek(type);
                expressions.RemoveAt(expressions.Count - 1);
                types.RemoveAt(types.Count - 1);
                return expression;
            }

        }

        
    }

    /// <summary>
    /// bool 类型表达式
    /// </summary>
    public class BooleanExpression : VariableExpression
    {
        readonly Func<IReadOnlyDictionary<string, int>, bool> asFunction;

        public BooleanExpression(string expression) : base(expression)
        {
            asFunction = Compile<bool>();
        }

        public bool Evaluate(IReadOnlyDictionary<string,int> symbols)
        {
            return asFunction(symbols);
        }
    }
}