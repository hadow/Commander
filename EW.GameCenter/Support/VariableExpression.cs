using System;
using System.Collections.Generic;


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
        public readonly string Expression;

        readonly HashSet<string> variables = new HashSet<string>();

        public IEnumerable<string> Variables { get { return variables; } }


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
    }

    public class BooleanExpression : VariableExpression
    {
        readonly Func<IReadOnlyDictionary<string, int>, bool> asFunction;

        public BooleanExpression(string expression) : base(expression)
        {

        }
    }
}