namespace Nett.Parser.Matchers
{
    internal static class SymbolsMatcher
    {
        internal static Token? TryMatch(LookaheadBuffer<char> cs)
        {
            Token token;
            if (cs.TryExpect('[')) { token = new Token(TokenType.LBrac, "["); }
            else if (cs.TryExpect(']')) { token = new Token(TokenType.RBrac, "]"); }
            else if (cs.TryExpect('=')) { token = new Token(TokenType.Assign, "="); }
            else if (cs.TryExpect(':')) { token = new Token(TokenType.Colon, ":"); }
            else if (cs.TryExpect(',')) { token = new Token(TokenType.Comma, ","); }
            else if (cs.TryExpect('.')) { token = new Token(TokenType.Dot, "."); }
            else if (cs.TryExpect('{')) { token = new Token(TokenType.LCurly, "{"); }
            else if (cs.TryExpect('}')) { token = new Token(TokenType.RCurly, "}"); }
            else
            {
                return null;
            }

            cs.Consume();
            return token;
        }
    }
}
