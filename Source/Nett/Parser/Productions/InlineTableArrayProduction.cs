namespace Nett.Parser.Productions
{
    internal static class InlineTableArrayProduction
    {
        public static TomlTableArray TryApply(ITomlRoot root, TokenBuffer tokens)
        {
            int pos = 0;
            if (!tokens.TryExpectAt(pos++, TokenType.LBrac)) { return null; }
            ictx.ConsumeAllNewlinesAndComments();
            if (!tokens.TryExpectAt(pos++, TokenType.LCurly)) { return null; }

            return Apply(root, tokens);
        }

        private static TomlTableArray Apply(ITomlRoot root, TokenBuffer tokens)
        {
            tokens.ExpectAndConsume(TokenType.LBrac);
            tokens.ConsumeAllNewlines();
            var preComments = CommentProduction.TryParsePreExpressionComments(tokens);

            var arr = new TomlTableArray(root);
            arr.AddComments(preComments);
            TomlTable tbl = null;
            while ((tbl = InlineTableProduction.TryApply(root, tokens)) != null) {
                arr.Add(tbl);
                var exprToken = tokens.Peek();
                if (root.Settings.AllowNonstandard)
                    tokens.ConsumeAllNewlines();
                var haveComma = tokens.TryExpectAndConsume(TokenType.Comma);
                if (root.Settings.AllowNonstandard) {
                    tokens.ConsumeAllNewlines();
                    tbl.AddComments(CommentProduction.TryParseComments(tokens, CommentLocation.Append));
                } else {
                    tbl.AddComments(CommentProduction.TryParseAppendExpressionComments(exprToken, tokens));
                }
                if (!haveComma)
                    break;
            }

            tokens.ExpectAndConsume(TokenType.RBrac);
            arr.AddComments(CommentProduction.TryParseComments(tokens, CommentLocation.Append));

            return arr;
        }
    }
}
