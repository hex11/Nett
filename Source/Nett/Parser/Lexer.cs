using System;
using System.Collections.Generic;
using Nett.Parser.Matchers;

namespace Nett.Parser
{
    internal sealed class Lexer
    {
        private static readonly Action NoAction = () => { };

        private readonly LexInput input;
        private readonly List<Token> lexed = new List<Token>();

        private Action<char> lexerState;

        public Lexer(string input)
        {
            this.input = new LexInput(input, this.LexLValue, this.LexRValue);
            this.lexerState = this.input.LexValueState;
        }

        private bool Eof
            => this.input.Eof;

        public List<Token> Lex()
        {
            this.LexAllInput();
            return this.lexed;
        }

        private void LexAllInput()
        {
            while (!this.Eof)
            {
                char c = this.Peek();

                var oldPos = this.input.Position;
                var oldState = this.lexerState;

                this.lexerState(c);

                if (oldState == this.lexerState && oldPos == this.input.Position)
                {
                    this.Consume();
                }
            }

            this.lexerState(LexInput.EofChar);
        }

        private void LexLValue(char c)
        {
            if (c == '[') { this.ExitState(TokenType.LBrac); }
            else if (c == ']') { this.ExitState(TokenType.RBrac); }
            else if (c == '}') { this.ExitState(TokenType.RCurly); }
            else if (c == ',') { this.ExitState(TokenType.Comma); }
            else if (c == '=') { this.ExitState(TokenType.Assign); }
            else if (c == '.') { this.ExitState(TokenType.Dot); }
            else if (c == '\r') { }
            else if (c == '\n') { this.ExitState(TokenType.NewLine); }
            else if (c == '#') { this.EnterState(this.LexComment, this.Skip); }
            else if (c == '\"') { this.EnterState(this.LexBasicString, this.Skip); }
            else if (c == '\'') { this.EnterState(this.LexLiteralString, this.Skip); }
            else if (c == LexInput.EofChar) { }
            else if (c.IsBareKeyChar()) { this.EnterState(this.LexBareKey); }
            else { this.EnterState(this.LexUnknown); }
        }

        private void LexBareKey(char c)
        {
            if (!c.IsBareKeyChar())
            {
                this.BreakState(TokenType.BareKey);
            }
        }

        private void LexRValue(char c)
        {
            if (this.TryLexStringRValue("true")) { this.BreakState(TokenType.Bool); }
            else if (this.TryLexStringRValue("false")) { this.BreakState(TokenType.Bool); }
            else if (this.TryLexStringRValue("+inf")) { this.BreakState(TokenType.Float); }
            else if (this.TryLexStringRValue("-inf")) { this.BreakState(TokenType.Float); }
            else if (this.TryLexStringRValue("inf")) { this.BreakState(TokenType.Float); }
            else if (this.TryLexStringRValue("+nan")) { this.BreakState(TokenType.Float); }
            else if (this.TryLexStringRValue("-nan")) { this.BreakState(TokenType.Float); }
            else if (this.TryLexStringRValue("nan")) { this.BreakState(TokenType.Float); }
            else if (c == '=') { this.ExitState(TokenType.Assign); }
            else if (c == '+' || c == '-') { this.EnterState(this.LexDecimal); }
            else if (c == '0') { this.EnterState(this.LexLeadingZeroRemainder); }
            else if (c.InRange('1', '9')) { this.EnterState(this.LexDecimalOrDateTime); }
            else if (c == '\"') { this.EnterState(this.LexBasicString, this.Skip); }
            else if (c == '\'') { this.EnterState(this.LexLiteralString, this.Skip); }
            else if (c == '\r') { }
            else if (c == '\n') { this.ExitState(TokenType.NewLine); }
            else if (c == '[') { this.ExitState(TokenType.LBrac); }
            else if (c == ']') { this.ExitState(TokenType.RBrac); }
            else if (c == '{') { this.ExitState(TokenType.LCurly); }
            else if (c == '}') { this.ExitState(TokenType.RCurly); }
            else if (c == ',') { this.ExitState(TokenType.Comma); }
            else if (c == '#') { this.EnterState(this.LexComment, this.Skip); }
            else { this.EnterState(this.LexUnknown); }
        }

        private bool TryLexStringRValue(string value)
        {
            if (this.PeekSring(value.Length) == value)
            {
                this.Consume(value.Length);
                return true;
            }

            return false;
        }

        private void LexLeadingZeroRemainder(char c)
        {
            if (c == 'x') { this.EnterState(this.LexHexNumber); }
            else if (c == 'o') { this.EnterState(this.LexOctalNumber); }
            else if (c == 'b') { this.EnterState(this.LexBinaryNumber); }
            else if (c == '.') { this.EnterState(this.LexFloatFractionalPartFirstDigit); }
            else if (c.IsNumChar()) { this.EnterState(this.LexDecimalOrDateTime); }
            else { this.BreakState(TokenType.Integer); }
        }

        private void LexDecimal(char c)
        {
            if (c == '.') { this.EnterState(this.LexFloatFractionalPartFirstDigit); }
            else if (c.IsExponent()) { this.EnterState(this.LexFloatExponentFirstDigitOrSign); }
            else if (!c.IsNumChar()) { this.BreakState(TokenType.Integer); }
        }

        private void LexDecimalOrDateTime(char c)
        {
            if (c == '.') { this.EnterState(this.LexFloatFractionalPartFirstDigit); }
            else if (c.IsExponent()) { this.EnterState(this.LexFloatExponentFirstDigitOrSign); }
            else if (c == '-') { this.LexLocalDate(); }
            else if (c == ':') { this.LexLocalTime(TokenType.Timespan); }
            else if (!c.IsNumChar()) { this.BreakState(TokenType.Integer); }
        }

        private void LexLocalDate()
        {
            if (this.PeekEmit().Length != "XXXX-".Length) { this.EmitUnknown(); }

            this.Expect(c => c.IsDigit());
            this.Expect(c => c.IsDigit());
            this.Expect(c => c == '-');
            this.Expect(c => c.IsDigit());
            this.Expect(c => c.IsDigit());

            bool isDateTimeSep = this.Peek() == 'T' || this.Peek() == ' ';

            if (isDateTimeSep && this.Peek(1).IsDigit() && this.Peek(2).IsDigit())
            {
                this.Consume(3);
                this.LexLocalTime(TokenType.DateTime);
            }
            else
            {
                this.BreakState(TokenType.DateTime);
            }
        }

        private void LexLocalTime(TokenType first)
        {
            this.Expect(c => c == ':');
            this.Expect(c => c.IsDigit());
            this.Expect(c => c.IsDigit());
            this.Expect(c => c == ':');
            this.Expect(c => c.IsDigit());
            this.Expect(c => c.IsDigit());

            if (this.Peek() == '.')
            {
                this.Consume();
                while (this.Peek().IsDigit())
                {
                    this.Consume();
                }
            }

            if (this.Peek() == 'Z')
            {
                this.Consume();
            }
            else if (this.TryExpect(c => c == '-' || c == '+'))
            {
                this.Expect(c => c.IsDigit());
                this.Expect(c => c.IsDigit());
                this.Expect(c => c == ':');
                this.Expect(c => c.IsDigit());
                this.Expect(c => c.IsDigit());
            }

            this.BreakState(first != TokenType.Unknown ? first : TokenType.Timespan);
        }

        private void LexHexNumber(char c)
        {
            if (!c.InRange('0', '9') && !c.InRange('a', 'f') && !c.InRange('A', 'F') && c != '_')
            {
                this.BreakState(TokenType.HexInteger);
            }
        }

        private void LexOctalNumber(char c)
        {
            if (!c.InRange('0', '7') && c != '_')
            {
                this.BreakState(TokenType.OctalInteger);
            }
        }

        private void LexBinaryNumber(char c)
        {
            if (!c.InRange('0', '1') && c != '_')
            {
                this.BreakState(TokenType.BinaryInteger);
            }
        }

        private void LexFloatFractionalPartFirstDigit(char c)
        {
            if (!c.IsDigit()) { this.EmitUnknown(); }
            else { this.EnterState(this.LexFloatFractionalPart); }
        }

        private void LexFloatFractionalPart(char c)
        {
            if (c.IsExponent()) { this.EnterState(this.LexFloatExponentFirstDigitOrSign); }
            else if (!c.IsNumChar()) { this.BreakState(TokenType.Float); }
        }

        private void LexFloatExponentFirstDigitOrSign(char c)
        {
            if (c.Is('+', '-')) { this.EnterState(this.LexFloatExponentFirstDigit); }
            else if (c.IsDigit()) { this.EnterState(this.LexFloatExponent); }
            else { this.EmitUnknown(); }
        }

        private void LexFloatExponentFirstDigit(char c)
        {
            if (c.IsDigit()) { this.EnterState(this.LexFloatExponent); }
            else { this.EmitUnknown(); }
        }

        private void LexFloatExponent(char c)
        {
            if (!c.IsNumChar()) { this.BreakState(TokenType.Float); }
        }

        private void LexBasicString(char c)
            => this.LexString(c, '\"', '\\');

        private void LexLiteralString(char c)
            => this.LexString(c, '\'', LexInput.EofChar);

        private void LexString(char c, char t, char escape)
        {
            if (this.PeekSequence(t, t))
            {
                this.Skip(2);

                if (t == '\'')
                {
                    if (this.Peek() == '\r') { this.Skip(); }
                    if (this.Peek() == '\n') { this.Skip(); }
                }

                this.EnterState(fc => this.LexMultilineString(c, t), NoAction);
            }
            else { this.EnterState(fc => this.LexSingleLineString(fc, t, escape), NoAction); }
        }

        private void LexSingleLineString(char c, char t, char escape)
        {
            if (c == escape) { this.Consume(2); }
            else if (c == t)
            {
                this.Skip();
                this.BreakState(t == '\'' ? TokenType.LiteralString : TokenType.String);
            }
        }

        private void LexMultilineString(char c, char t)
        {
            if (this.PeekSequence(t, t, t) && this.Peek(3) != t)
            {
                this.Skip(3);
                this.BreakState(t == '\'' ? TokenType.MultilineLiteralString : TokenType.MultilineString);
            }
        }

        private void LexComment(char c)
        {
            if (c.Is('\r', '\n', LexInput.EofChar))
            {
                this.BreakState(TokenType.Comment);
            }
        }

        private void LexUnknown(char c)
        {
            if (c.IsWhitespaceChar())
            {
                this.BreakState(TokenType.Unknown);
            }
        }

        private void EnterState(Action<char> state)
        {
            this.Consume();
            this.SetCurrentState(state);
        }

        private void EnterState(Action<char> state, Action enterAction)
        {
            enterAction();
            this.SetCurrentState(state);
        }

        private Action<char> GetFollowValueSate()
            => this.input.LexValueState;

        private void SetCurrentState(Action<char> state)
        {
            this.lexerState = state;
        }

        private void ExitState(TokenType type)
        {
            this.Consume();
            this.BreakState(type);
        }

        private void BreakState(TokenType type)
        {
            this.Emit(type);
            this.lexerState = this.input.LexValueState;
        }

        private void Emit(TokenType type)
        {
            this.lexed.AddRange(this.input.Emit(type));
        }

        private void EmitUnknown()
            => this.EnterState(this.LexUnknown);

        private char Consume()
            => this.input.Consume();

        private string Consume(int n)
            => this.input.Consume(n);

        private char Peek()
            => this.input.Peek();

        private char Peek(int n)
            => this.input.Peek(n);

        private string PeekEmit()
            => this.input.PeekEmit();

        private void Expect(Func<char, bool> expectation)
        {
            if (!expectation(this.Peek())) { this.EmitUnknown(); }
            else { this.Consume(); }
        }

        private bool TryExpect(Func<char, bool> expecation)
        {
            bool result = expecation(this.Peek());
            if (result)
            {
                this.Consume();
            }

            return result;
        }

        private bool PeekSequence(params char[] c)
        {
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] != this.input.Peek(i))
                {
                    return false;
                }
            }

            return true;
        }

        private void Skip()
            => this.Skip(1);

        private void Skip(int n)
            => this.input.Skip(n);

        private string PeekSring(int len)
             => this.input.PeekString(len);
    }
}
