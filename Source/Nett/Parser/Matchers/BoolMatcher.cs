namespace Nett.Parser.Matchers
{
    internal static class BoolMatcher
    {
        private const string F = "false";
        private const string T = "true";
        private const string Null = "null";

        public static Token? TryMatch(CharBuffer cs)
        {
            if (cs.TryExpect(T))
            {
                cs.Consume(T.Length);
                return new Token(TokenType.Bool, T);
            }
            else if (cs.TryExpect(F))
            {
                cs.Consume(F.Length);
                return new Token(TokenType.Bool, F);
            }
            else if (cs.TryExpect(Null))
            {
                cs.Consume(Null.Length);
                return new Token(TokenType.Null, Null);
            }
            else
            {
                return null;
            }
        }
    }
}
