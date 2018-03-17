using System;
using System.Collections.Generic;
using System.Text;
using Nett.Parser.Matchers;

namespace Nett.Parser
{
    internal static class LexInputExtensions
    {
        public static string Consume(this LexInput input, int len)
        {
            var sb = new StringBuilder(len);
            for (int i = 0; i < len; i++)
            {
                sb.Append(input.Consume());
            }

            return sb.ToString();
        }
    }

    internal sealed class LexInput
    {
        public const char EofChar = char.MaxValue;

        private const int MaxLa = 256;

        private readonly StringBuilder emitBuffer = new StringBuilder(1024);
        private readonly string input;
        private readonly int originalLength;
        private readonly ValueScopeTracker scopeTracker;

        private int tokenStart = 0;
        private int index = 0;

        private int line = 1;
        private int column = 1;

        private int tokenLine = 1;
        private int tokenCol = 1;

        public LexInput(string input, Action<char> lvalueAction, Action<char> rValueAction)
        {
            this.originalLength = input.Length;
            this.input = input + new string(EofChar, MaxLa);
            this.scopeTracker = new ValueScopeTracker(lvalueAction, rValueAction);
        }

        public int Position => this.index;

        public Action<char> LexValueState
            => this.scopeTracker.ScopeAction;

        public bool Eof
            => this.index >= this.originalLength;

        public char Peek()
            => this.input[this.index];

        public char Peek(int n)
            => this.input[this.index + n];

        public string PeekString(int n)
        {
            int len = this.index + n < this.input.Length
                ? n
                : this.input.Length - this.index;

            return this.input.Substring(this.index, len);
        }

        public string PeekEmit()
            => this.emitBuffer.ToString();

        public IEnumerable<Token> Emit(TokenType type)
        {
            yield return new Token(type, this.PeekEmit())
            {
                line = this.tokenLine,
                col = this.tokenCol,
            };

            foreach (var t in this.ConsumeWhitepsaces())
            {
                yield return t;
            }

            this.emitBuffer.Clear();
            this.scopeTracker.Emit(type);
            this.SetTokenStartLocation();
        }

        public char Consume()
        {
            char c = this.Advance();
            this.emitBuffer.Append(c);
            return c;
        }

        public void Skip(int n = 1)
        {
            for (int i = 0; i < n; i++)
            {
                this.Advance();
            }
        }

        private char Advance()
        {
            this.column++;

            var c = this.input[this.index++];
            if (c == '\n')
            {
                this.AdvanceLine();
            }

            return c;
        }

        private void SetTokenStartLocation()
        {
            this.tokenLine = this.line;
            this.tokenCol = this.column;
            this.tokenStart = this.index;
        }

        private IEnumerable<Token> ConsumeWhitepsaces()
        {
            char c;
            while ((c = this.Peek()).IsWhitespaceChar())
            {
                if (c == '\n')
                {
                    yield return Token.NewLine(this.line, this.column);
                }

                this.Consume();
            }
        }

        private void AdvanceLine()
        {
            this.line++;
            this.column = 1;
        }
    }
}
