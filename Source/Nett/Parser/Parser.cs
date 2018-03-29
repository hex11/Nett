namespace Nett.Parser
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Nett.Parser.Productions;
    using static System.Diagnostics.Debug;

    internal sealed class Parser
    {
        private readonly TomlSettings settings;
        private readonly Stream stream;

        private List<Token> tokens;

        public Parser(Stream s, TomlSettings settings)
        {
            Assert(settings != null);

            this.stream = s;
            this.settings = settings;
        }

        private TokenBuffer Tokens { get; set; }

        public static Exception CreateParseError(FilePosition pos, string message)
            => new Exception($"Line {pos.Line}, Column {pos.Column}: {message}");

        public static Exception CreateParseError(Token token, string message)
        {
            // There was a lexer error. So probably the lexer error contains more useful error information
            if (token.type == TokenType.Unknown && token.errorHint != null)
            {
                message = token.errorHint;
            }

            return CreateParseError(new FilePosition { Line = token.line, Column = token.col }, message);
        }

        public TomlTable Parse()
        {
            var reader = new StreamReader(this.stream);
            var input = reader.ReadToEnd();
            var lexer = new Lexer(input);
            this.tokens = lexer.Lex();
            this.Tokens = new TokenBuffer(this.tokens.ToArray());

            return this.Toml();
        }

        private TomlTable Toml()
        {
            var root = new TomlTable.RootTable(this.settings) { IsDefined = true };
            TomlTable current = root;

            if (this.settings.AllowNonstandard)
            {
                this.Tokens.ConsumeAllNewlines();
                var inlineTable = InlineTableProduction.TryApply(root, this.Tokens);
                if (inlineTable != null)
                {
                    this.Tokens.ConsumeAllNewlines();
                    inlineTable.AddComments(CommentProduction.TryParseComments(this.Tokens, CommentLocation.Append));
                    if (!this.Tokens.End)
                    {
                        throw new Exception();
                    }

                    return inlineTable;
                }
            }

            while (!this.Tokens.End)
            {
                current = ExpressionsProduction.TryApply(current, root, this.Tokens);
                if (current == null)
                {
                    if (!this.Tokens.End)
                    {
                        throw new Exception();
                    }

                    break;
                }
            }

            return root;
        }
    }
}
