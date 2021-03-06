﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Expressions =  System.Linq.Expressions;
using System.IO;
namespace EW.Support
{
    public abstract class VariableExpression
    {
        /// <summary>
        /// 字符类别
        /// </summary>
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

        [Flags]
        enum Sides
        {
            //Value type
            None =0,


            Left =1,

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

            // varying values
            Number,
            Variable,

            //operators
            OpenParen,
            CloseParen,
            Not,
            Negate,
            OnesComplement,//二进制反码

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
            Divide,
            Modulo, //取模计算

            Invalid,
        }

        /// <summary>
        /// 优先级
        /// </summary>
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

            public TokenTypeInfo(string symbol,Precedence precedence,Grouping opens,Grouping closes = Grouping.None,Associativity associativity = Associativity.Left)
            {
                Symbol = symbol;
                Precedence = precedence;
                WhitespaceSides = Sides.None;
                OperandSides = opens == Grouping.None ? 
                    (closes == Grouping.None ? Sides.None : Sides.Left) : 
                    (closes == Grouping.None ? Sides.Right : Sides.Both);

                Associativity = associativity;

                Opens = opens;
                Closes = closes;

            }

            public TokenTypeInfo(string symbol,Precedence precedence,Sides operandSides,
                Sides whitespaceSides,
                Associativity associativity = Associativity.Left,
                Grouping opens = Grouping.None,Grouping closes = Grouping.None)
            {
                Symbol = symbol;
                Precedence = precedence;
                OperandSides = operandSides;
                WhitespaceSides = whitespaceSides;
                Associativity = associativity;
                Opens = opens;
                Closes = closes;
            }



        }

        public static readonly IReadOnlyDictionary<string, int> NoVariables = new ReadOnlyDictionary<string, int>(new Dictionary<string, int>());

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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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
                    case TokenType.Number:
                        yield return new TokenTypeInfo("(<number>)", Precedence.Value);
                        continue;
                    case TokenType.Variable:
                        yield return new TokenTypeInfo("(<variable>)", Precedence.Value);
                        continue;
                    case TokenType.OpenParen:
                        yield return new TokenTypeInfo("(", Precedence.Parens, Grouping.Parens);
                        continue;
                    case TokenType.CloseParen:
                        yield return new TokenTypeInfo(")", Precedence.Parens, Grouping.None, Grouping.Parens);
                        continue;
                    case TokenType.Not:
                        yield return new TokenTypeInfo("!", Precedence.Unary, Sides.Right, Associativity.Right);
                        continue;
                    case TokenType.OnesComplement:
                        yield return new TokenTypeInfo("~", Precedence.Unary, Sides.Right, Associativity.Right);
                        continue;
                    case TokenType.Negate://取反
                        yield return new TokenTypeInfo("-", Precedence.Unary, Sides.Right, Associativity.Right);
                        continue;
                    case TokenType.And:
                        yield return new TokenTypeInfo("&&", Precedence.And, Sides.Both, Sides.Both);
                        continue;
                    case TokenType.Or:
                        yield return new TokenTypeInfo("||", Precedence.Or, Sides.Both, Sides.Both);
                        continue;
                    case TokenType.Equals:
                        yield return new TokenTypeInfo("==", Precedence.Equality, Sides.Both, Sides.Both);
                        continue;
                    case TokenType.NotEquals:
                        yield return new TokenTypeInfo("!=", Precedence.Equality, Sides.Both, Sides.Both);
                        continue;
                    case TokenType.LessThan:
                        yield return new TokenTypeInfo("<", Precedence.Relation, Sides.Both, Sides.Both);
                        continue;
                    case TokenType.LessThanOrEqual:
                        yield return new TokenTypeInfo("<=", Precedence.Relation, Sides.Both, Sides.Both);
                        continue;
                    case TokenType.GreaterThan:
                        yield return new TokenTypeInfo(">", Precedence.Relation, Sides.Both, Sides.Both);
                        continue;
                    case TokenType.GreaterThanOrEqual:
                        yield return new TokenTypeInfo(">=", Precedence.Relation, Sides.Both, Sides.Both);
                        continue;
                    case TokenType.Add:
                        yield return new TokenTypeInfo("+", Precedence.Addition, Sides.Both, Sides.Both);
                        continue;
                    case TokenType.Subtract:
                        yield return new TokenTypeInfo("-", Precedence.Addition, Sides.Both, Sides.Both);
                        continue;
                    case TokenType.Multiply:
                        yield return new TokenTypeInfo("*", Precedence.Multiplication, Sides.Both, Sides.Both);
                        continue;
                    case TokenType.Divide:
                        yield return new TokenTypeInfo("/", Precedence.Multiplication, Sides.Both, Sides.Both);
                        continue;
                    case TokenType.Modulo:
                        yield return new TokenTypeInfo("%", Precedence.Multiplication, Sides.Both, Sides.Both);
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
                case '&':
                case '*':
                case '(':
                case ')':
                case '+':
                case '=':
                case '[':
                case ']':
                case '{':
                case '}':
                case '|':
                case ':':
                case ';':
                case '\'':
                case '"':
                case '<':
                case '>':
                case '?':
                case ',':
                case '/':
                    return CharClass.Operator;

                case '.':
                case '$':
                case '-':
                case '@':
                    return CharClass.Mixed;
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

            for (var i = 0; ;)
            {
                var token = Token.GetNext(Expression, ref i, lastToken != null ? lastToken.Type : TokenType.Invalid);
                if(token == null)
                {
                    //Sanity check parsed tree
                    if (lastToken == null)
                        throw new InvalidDataException("Empty expression");
                    //Expressions can't end with a binary or unary prefix operation
                    //表达式不能以二进制或一元前缀操作结束 
                    if (lastToken.RightOperand)
                        throw new InvalidDataException("Missing value or sub-expression at end for '{0}' operator".F(lastToken.Symbol));
                    break;
                }

                if(token.Closes != Grouping.None)
                {
                    if (currentOpeners.Count == 0)
                        throw new InvalidDataException("Unmatched closing parenthesis at index  {0}".F(token.Index));

                    currentOpeners.Pop();
                }

                if (token.Opens != Grouping.None)
                    currentOpeners.Push(token);

                if(lastToken  == null)
                {
                    //Expressions can't end with a binary or unary prefix operation
                    if (token.LeftOperand)
                    {
                        throw new InvalidDataException("Missing value or sub-expression at beginning for '{0}' operator".F(token.Symbol));
                    }
                }
                else
                {
                    if(lastToken.Opens != Grouping.None && token.Closes != Grouping.None)
                    {
                        throw new InvalidDataException("Empty parenthesis at index {0}".F(lastToken.Index));
                    }

                    if(lastToken.RightOperand == token.LeftOperand)
                    {
                        if (lastToken.RightOperand)
                            throw new InvalidDataException("Missing value or sub-expression or there is an extra operator '{0}' at index {1} or '{2}' at  index {3}".F(
                                lastToken.Symbol,lastToken.Index,token.Symbol,token.Index
                                ));
                        throw new InvalidDataException("Missing binary operation before '{0}' at index {1}".F(token.Symbol, token.Index));

                    }
                }
                if (token.Type == TokenType.Variable)
                    variables.Add(token.Symbol);

                tokens.Add(token);
                lastToken = token;


            }

            if (currentOpeners.Count > 0)
                throw new InvalidDataException("Unclosed opening parenthesis at index {0}".F(currentOpeners.Peek().Index));

            return new Compiler().Build(ToPostfix(tokens).ToArray(), resultType);
        }


        static int ParseSymbol(string symbol,IReadOnlyDictionary<string,int> symbols)
        {
            int value;
            symbols.TryGetValue(symbol, out value);
            return value;
        }

        static IEnumerable<Token> ToPostfix(IEnumerable<Token> tokens)
        {
            var s = new Stack<Token>();
            foreach(var t in tokens)
            {
                if (t.Opens != Grouping.None)
                    s.Push(t);
                else if (t.Closes != Grouping.None)
                {
                    Token temp;
                    while (!((temp = s.Pop()).Opens != Grouping.None))
                        yield return temp;

                }
                else if (t.OperandSides == Sides.None)
                    yield return t;
                else
                {
                    while(s.Count>0 && ((t.Associativity == Associativity.Right && t.Precedence < s.Peek().Precedence) 
                        || (t.Associativity == Associativity.Left && t.Precedence <= s.Peek().Precedence)))
                    {
                        yield return s.Pop();
                    }

                    s.Push(t);
                }

            }
            while (s.Count > 0)
                yield return s.Pop();
        }

        /// <summary>
        /// 
        /// </summary>
        class Token
        {

            public readonly TokenType Type;
            public readonly int Index;
            
            public virtual string Symbol { get { return TokenTypeInfos[(int)Type].Symbol; } }

            public int Precedence { get { return (int)TokenTypeInfos[(int)Type].Precedence; } }
            public Sides OperandSides { get { return TokenTypeInfos[(int)Type].OperandSides; } }

            public Associativity Associativity { get { return TokenTypeInfos[(int)Type].Associativity; } }

            public Grouping Opens { get { return TokenTypeInfos[(int)Type].Opens; } }

            public Grouping Closes { get { return TokenTypeInfos[(int)Type].Closes; } }

            /// <summary>
            /// 右操作数
            /// </summary>
            public bool RightOperand
            {
                get
                {
                    return ((int)TokenTypeInfos[(int)Type].OperandSides & (int)Sides.Right) != 0;
                }
            }

            /// <summary>
            /// 左操作数
            /// </summary>
            public bool LeftOperand
            {
                get
                {
                    return ((int)TokenTypeInfos[(int)Type].OperandSides & (int)Sides.Left) != 0;
                }
            }



            public Token(TokenType type,int index)
            {
                Type = type;
                Index = index;
            }

            static bool ScanIsNumber(string expression,int start,ref int i)
            {
                var cc = CharClassOf(expression[i]);

                if(cc == CharClass.Digit)
                {
                    i++;
                    for (; i < expression.Length; i++)
                    {
                        cc = CharClassOf(expression[i]);
                        if(cc!= CharClass.Digit)
                        {
                            if (cc != CharClass.Whitespace && cc != CharClass.Operator && cc != CharClass.Mixed)
                                throw new InvalidDataException("Number {0} and variable merged at index {1}".F(int.Parse(expression.Substring(start, i - start)), start));

                            return true;
                        }
                    }
                    return true;
                }
                return false;
            }

            public static TokenType GetNextType(string expression,ref int i,TokenType lastType = TokenType.Invalid)
            {
                var start = i;

                switch (expression[i])
                {
                    case '!':
                        i++;
                        if (i < expression.Length && expression[i] == '=')
                        {
                            i++;
                            return TokenType.NotEquals;
                        }
                        return TokenType.Not;
                    case '<':
                        i++;
                        if (i < expression.Length && expression[i] == '=')
                        {
                            i++;
                            return TokenType.LessThanOrEqual;
                        }
                        return TokenType.LessThan;
                    case '>':
                        i++;
                        if (i < expression.Length && expression[i] == '=')
                        {
                            i++;
                            return TokenType.GreaterThanOrEqual;
                        }
                        
                        return TokenType.GreaterThan;
                    case '=':
                        i++;
                        if (i < expression.Length && expression[i] == '=')
                        {
                            i++;
                            return TokenType.Equals;
                        }
                        throw new InvalidDataException("Unexpected character '=' at index {0} - should it b '==' ?".F(start));
                    case '&':
                        i++;
                        if (i < expression.Length&& expression[i] == '&'){
                            i++;
                            return TokenType.And;
                        }
                        throw new InvalidDataException("Unexpected character '&' at index {0} - should it b '&&' ?".F(start));
                    case '|':
                        i++;
                        if (i < expression.Length&& expression[i] == '|'){
                            i++;
                            return TokenType.Or;
                        }
                        throw new InvalidDataException("Unexpected character '|' at index {0} - should it b '||' ?".F(start));
                    case '(':
                        i++;
                        return TokenType.OpenParen;
                    case ')':
                        i++;
                        return TokenType.CloseParen;
                    case '~':
                        i++;
                        return TokenType.OnesComplement;
                    case '+':
                        i++;
                        return TokenType.Add;
                    case '-':

                        return TokenType.Subtract;
                    case '*':
                        i++;
                        return TokenType.Multiply;
                    case '/':
                        i++;
                        return TokenType.Divide;
                    case '%':
                        i++;
                        return TokenType.Modulo;
                }

                if (ScanIsNumber(expression, start, ref i))
                    return TokenType.Number;

                var cc = CharClassOf(expression[start]);

                if (cc != CharClass.Id)
                    throw new InvalidDataException("Invalid character '{0}' at index {1}".F(expression[i], start));

                for(i = start; i < expression.Length; i++)
                {
                    cc = CharClassOf(expression[i]);
                    if (cc == CharClass.Whitespace || cc == CharClass.Operator)
                        return VariableOrKeyword(expression, start, ref i);
                }

                return VariableOrKeyword(expression, start, ref i);

            }

            static TokenType VariableOrKeyword(string expression,int start,ref int i)
            {
                if (CharClassOf(expression[i - 1]) == CharClass.Mixed)
                    throw new InvalidDataException("Invalid identifier end character at index {0} for '{1}'".F(i - 1, expression.Substring(start, i - start)));

                return VariableOrKeyword(expression, start, i - start);
            }

            static TokenType VariableOrKeyword(string expression,int start,int length)
            {
                var i = start;
                if (length == 4 && expression[i++] == 't' && expression[i++] == 'r' && expression[i++] == 'u' && expression[i++] == 'e')
                    return TokenType.True;
                if (length == 5 && expression[i++] == 'f' && expression[i++] == 'a' && expression[i++] == 'l' && expression[i++] == 's' && expression[i++] == 'e')
                    return TokenType.False;

                return TokenType.Variable;
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

                var type = GetNextType(expression, ref i, lastType);

                if (!whitespaceBefore && RequiresWhitespaceBefore(type))
                    throw new InvalidDataException("Missing whitespace at index {0}, before '{1}' operator.".F(i, GetTokenSymbol(type)));

                switch (type)
                {
                    case TokenType.Number:
                        return new NumberToken(start, expression.Substring(start, i - start));
                    case TokenType.Variable:
                        return new VariableToken(start, expression.Substring(start, i - start));
                    default:
                        return new Token(type, start);
                }
            }


            static bool RequiresWhitespaceBefore(TokenType type)
            {
                return ((int)TokenTypeInfos[(int)type].WhitespaceSides & (int)Sides.Left) != 0;
            }

            static string GetTokenSymbol(TokenType type)
            {
                return TokenTypeInfos[(int)type].Symbol;
            }
        }
        
        class NumberToken : Token
        {
            public readonly int Value;
            readonly string symbol;

            public override string Symbol { get { return symbol; } }

            public NumberToken(int index,string symbol) : base(TokenType.Number, index)
            {
                Value = int.Parse(symbol);
                this.symbol = symbol;
            }

        }

        class VariableToken : Token
        {
            public readonly string Name;
            public override string Symbol { get { return Name; } }

            public VariableToken(int index,string symbol) : base(TokenType.Variable, index) { Name = symbol; }
        }

        class Compiler
        {

            readonly AstStack ast = new AstStack();

            public Expression Build(Token[] postfix,ExpressionT resultType)
            {
                foreach(var t in postfix)
                {
                    switch (t.Type)
                    {
                        case TokenType.And:
                            {
                                var y = ast.Pop(ExpressionT.Bool);
                                var x = ast.Pop(ExpressionT.Bool);
                                ast.Push(Expressions.Expression.And(x, y));
                                continue;
                            }
                        case TokenType.Or:
                            {
                                var y = ast.Pop(ExpressionT.Bool);
                                var x = ast.Pop(ExpressionT.Bool);
                                ast.Push(Expressions.Expression.Or(x, y));
                                continue;
                            }
                        case TokenType.Add:
                            {
                                var y = ast.Pop(ExpressionT.Bool);
                                var x = ast.Pop(ExpressionT.Bool);
                                ast.Push(Expressions.Expression.Add(x, y));
                                continue;
                            }
                        case TokenType.GreaterThan:
                            {
                                var y = ast.Pop(ExpressionT.Int);
                                var x = ast.Pop(ExpressionT.Int);
                                ast.Push(Expressions.Expression.GreaterThan(x, y));
                                continue;
                            }
                        case TokenType.GreaterThanOrEqual:
                            {
                                var y = ast.Pop(ExpressionT.Int);
                                var x = ast.Pop(ExpressionT.Int);
                                ast.Push(Expressions.Expression.GreaterThanOrEqual(x, y));
                                continue;
                            }
                        case TokenType.LessThan:
                            {
                                var y = ast.Pop(ExpressionT.Int);
                                var x = ast.Pop(ExpressionT.Int);
                                ast.Push(Expressions.Expression.LessThan(x, y));
                                continue;
                            }
                        case TokenType.LessThanOrEqual:
                            {
                                var y = ast.Pop(ExpressionT.Int);
                                var x = ast.Pop(ExpressionT.Int);
                                ast.Push(Expressions.Expression.LessThanOrEqual(x, y));
                                continue;
                            }
                        case TokenType.False:
                            {
                                ast.Push(False);
                                continue;
                            }
                        case TokenType.True:
                            {
                                ast.Push(True);
                                continue;
                            }
                        case TokenType.Number:
                            {
                                ast.Push(Expressions.Expression.Constant(((NumberToken)t).Value));
                                continue;
                            }
                        case TokenType.Variable:
                            {
                                var symbol = Expressions.Expression.Constant(((VariableToken)t).Symbol);
                                Func<string, IReadOnlyDictionary<string, int>, int> parseSymbol = ParseSymbol;
                                ast.Push(Expressions.Expression.Call(parseSymbol.Method, symbol, SymbolsParam));
                                continue;
                            }
                        case TokenType.Not:
                            {
                                ast.Push(Expressions.Expression.Not(ast.Pop(ExpressionT.Bool)));
                                continue;
                            }
                        case TokenType.NotEquals:
                            {
                                var y = ast.Pop(ExpressionT.Int);
                                var x = ast.Pop(ExpressionT.Int);
                                ast.Push(Expressions.Expression.NotEqual(x, y));
                                continue;
                            }
                        case TokenType.Equals:
                            {
                                var y = ast.Pop(ExpressionT.Int);
                                var x = ast.Pop(ExpressionT.Int);
                                ast.Push(Expressions.Expression.Equal(x, y));
                                continue;
                            }
                            
                        default:
                            throw new InvalidProgramException("ConditionExpression.Compile() is missing an expression builder for TokenType.{0}".F(Enum<TokenType>.GetValues()[(int)t.Type]));

                    }
                }
                return ast.Pop(resultType);
            }
        }
        static Expression IfThenElse(Expression test,Expression ifTrue,Expression ifFalse)
        {
            return Expressions.Expression.Condition(test, ifTrue, ifFalse);
        }

        /// <summary>
        /// 
        /// </summary>
        class AstStack
        {
            readonly List<Expression> expressions = new List<Expressions.Expression>();
            readonly List<ExpressionT> types = new List<ExpressionT>();


            static Expression AsBool(Expression expression)
            {
                return Expressions.Expression.NotEqual(expression, Zero);
            }


            public Expression Peek(ExpressionT toType)
            {
                var fromType = types[types.Count - 1];
                var expression = expressions[expressions.Count - 1];
                if (toType == fromType)
                    return expression;

                switch (toType)
                {
                    case ExpressionT.Bool:
                        return IfThenElse(AsBool(expression), True, False);
                    case ExpressionT.Int:
                        return IfThenElse(expression, One, Zero);
                }

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

    public class IntegerExpression : VariableExpression
    {
        readonly Func<IReadOnlyDictionary<string, int>, int> asFunction;

        public IntegerExpression(string expression) : base(expression)
        {
            asFunction = Compile<int>();
        }

        public int Evaluate(IReadOnlyDictionary<string,int> symbols)
        {
            return asFunction(symbols);
        }
    }
}