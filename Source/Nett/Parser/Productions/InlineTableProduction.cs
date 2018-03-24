namespace Nett.Parser.Productions
{
    internal static class InlineTableProduction
    {
        public static TomlTable Apply(ITomlRoot root, TokenBuffer tokens)
        {
            TomlTable inlineTable = new TomlTable(root, TomlTable.TableTypes.Inline);

            tokens.ExpectAndConsume(TokenType.LCurly);
            if (root.Settings.AllowNonstandard) {
                tokens.ConsumeAllNewlines();
                TomlObject lastVal = null;
                while (!tokens.TryExpect(TokenType.RCurly)) {
                    var preComments = CommentProduction.TryParsePreExpressionComments(tokens);
                    var exprToken = tokens.Peek();
                    var kvp = KeyValuePairProduction.Apply(root, tokens);
                    lastVal = kvp.Value;
                    var row = inlineTable.AddRow(kvp.Key, kvp.Value);
                    row.AddComments(preComments);
                    row.AddComments(CommentProduction.TryParseAppendExpressionComments(exprToken, tokens));
                    if (tokens.TryExpect(TokenType.Comma) || tokens.TryExpect(TokenType.NewLine)) {
                        var t = tokens.Consume().type;
                        if (t == TokenType.NewLine) {
                            tokens.ConsumeAllNewlines();
                        }
                        row.AddComments(CommentProduction.TryParseComments(tokens, CommentLocation.Append));
                    } else {
                        break;
                    }
                }

                tokens.ConsumeAllNewlines();
                var comments = CommentProduction.TryParsePreExpressionComments(tokens);
                (lastVal ?? inlineTable as TomlObject).AddComments(comments);
            } else {
                while (!tokens.TryExpect(TokenType.RCurly)) {
                    var exprToken = tokens.Peek();
                    var kvp = KeyValuePairProduction.Apply(root, tokens);
                    var row = inlineTable.AddRow(kvp.Key, kvp.Value);
                    if (!tokens.TryExpectAndConsume(TokenType.Comma))
                        break;
                }
            }

            tokens.ExpectAndConsume(TokenType.RCurly);
            return inlineTable;
        }

        public static TomlTable TryApply(ITomlRoot root, TokenBuffer tokens) =>
                    tokens.TryExpect(TokenType.LCurly)
                ? Apply(root, tokens)
                : null;
    }
}
