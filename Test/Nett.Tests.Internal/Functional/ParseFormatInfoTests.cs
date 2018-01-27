﻿using System;
using System.Linq;
using FluentAssertions;
using Nett.Tests.Util;

namespace Nett.Tests.Internal.Functional
{
    public sealed class ParseFormatInfoTests
    {
        private const string FuncParseFmtInfo = "Parse format info";

        [FFact(FuncParseFmtInfo, "When append comment has whitespace, parse info has corresponding whitespace")]
        public void ReadTable_WhenAppendCommentHasPrependedWhitespace_ParseInfoHasSameWhitespace()
        {
            // Arrange
            const string WS = "        ";
            string tml = $"x = 1{WS}# Comment";

            // Act
            var tbl = Toml.ReadString(tml);

            // Assert
            var pi = tbl["x"].Comments.ElementAt(0).ParseInfo;
            pi.Whitespace.Should().Be(WS);
        }

        [FFact(FuncParseFmtInfo, "When prepend comment has whitespace, parse info has corresponding whitespace")]
        public void ReadTable_WhenPrependCommentHasWhitespace_ParseInfoHasSameWhitespace()
        {
            // Arrange
            const string WS = "        ";
            string tml = $@"{WS}# Comment
x = 1";

            // Act
            var tbl = Toml.ReadString(tml);

            // Assert
            var pi = tbl["x"].Comments.ElementAt(0).ParseInfo;
            pi.Whitespace.Should().Be(WS);
        }

        [FFact(FuncParseFmtInfo, "When KVP-row has whitespace, parse info has corresponding whitespace")]
        public void ReadTable_WhenKeyValueRowHasWhitespace_ParseInfoHasSameWhitespace()
        {
            // Arrange
            string WS = $"{Environment.NewLine}{Environment.NewLine}    ";
            string tml = $@"x = 1{WS}y = 2";

            // Act
            var tbl = Toml.ReadString(tml);

            // Assert
            var kvp = tbl.InternalRows.Where(r => r.Key.Value == "y").Single();
            kvp.Key.ParseInfo.Whitespace.Should().Be(WS);
        }

    }
}
