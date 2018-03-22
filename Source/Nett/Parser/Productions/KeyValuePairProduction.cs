namespace Nett.Parser.Productions
{
    using System;
    using System.Collections.Generic;

    internal static class KeyValuePairProduction
    {
        public static KeyValuePair<TomlKey, TomlObject> Apply(ITomlRoot root, TokenBuffer tokens)
        {
            var key = KeyProduction.Apply(tokens);

            if (!(root.Settings.AllowNonstandard && tokens.TryExpectAndConsume(TokenType.Colon)))
            {
                tokens.ExpectAndConsume(TokenType.Assign);
            }

            var inlineTableArray = InlineTableArrayProduction.TryApply(root, tokens);
            if (inlineTableArray != null)
            {
                return new KeyValuePair<TomlKey, TomlObject>(key, inlineTableArray);
            }

            var inlineTable = InlineTableProduction.TryApply(root, tokens);
            if (inlineTable != null)
            {
                return new KeyValuePair<TomlKey, TomlObject>(key, inlineTable);
            }

            var value = ValueProduction.Apply(root, tokens);
            return new KeyValuePair<TomlKey, TomlObject>(key, value);
        }
    }
}
