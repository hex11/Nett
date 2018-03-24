namespace Nett.Parser.Productions
{
    internal static class InlineTableArrayProduction
    {
        public static TomlTableArray TryApply(ITomlRoot root, TokenBuffer tokens)
        {
            var ictx = tokens.GetImaginaryContext();
            if (!ictx.TryExpectAndConsume(TokenType.LBrac)) { return null; }
            ictx.ConsumeAllNewlinesAndComments();
            if (!ictx.TryExpectAndConsume(TokenType.LCurly)) { return null; }

            return Apply(root, tokens);
        }

        private static TomlTableArray Apply(ITomlRoot root, TokenBuffer tokens)
        {
            tokens.ExpectAndConsume(TokenType.LBrac);
            tokens.ConsumeAllNewlines();
            var preComments = CommentProduction.TryParsePreExpressionComments(tokens);

            var arr = new TomlTableArray(root);
            TomlTable tbl;
            while ((tbl = InlineTableProduction.TryApply(root, tokens)) != null) {
                if (preComments != null) {
                    tbl.AddComments(preComments);
                    preComments = null;
                }
                arr.Add(tbl);
                var exprToken = tokens.Peek();
                if (root.Settings.AllowNonstandard)
                    tokens.ConsumeAllNewlines();
                var haveComma = tokens.TryExpectAndConsume(TokenType.Comma);
                tbl.AddComments(CommentProduction.TryParseAppendExpressionComments(exprToken, tokens));
                tokens.ConsumeAllNewlines();
                tbl.AddComments(CommentProduction.TryParseComments(tokens, CommentLocation.Append));
                if (!haveComma)
                    break;
            }

            tokens.ExpectAndConsume(TokenType.RBrac);
            arr.AddComments(CommentProduction.TryParseComments(tokens, CommentLocation.Append));

            return arr;
        }
    }
}
