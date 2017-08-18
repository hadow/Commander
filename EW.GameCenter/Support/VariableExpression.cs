using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Expressions =  System.Linq.Expressions;

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

        enum TokenType
        {
            //fixed values
            False,
            True,

            //operators
            OpenParen,
            And,
            Or,
            Equals,
            NotEquals,
            LessThan,

        }
        public readonly string Expression;

        readonly HashSet<string> variables = new HashSet<string>();

        public IEnumerable<string> Variables { get { return variables; } }

        static readonly ParameterExpression SymbolsParam = Expressions.Expression.Parameter(typeof(IReadOnlyDictionary<string, int>), "symbols");
        static readonly ConstantExpression Zero = Expressions.Expression.Constant(0);
        static readonly ConstantExpression One = Expressions.Expression.Constant(1);
        static readonly ConstantExpression False = Expressions.Expression.Constant(false);
        static readonly ConstantExpression True = Expressions.Expression.Constant(true);

        public VariableExpression(string expression)
        {
            Expression = expression;
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


        class Token
        {

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