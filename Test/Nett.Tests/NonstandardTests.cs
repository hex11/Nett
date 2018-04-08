using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Nett.Tests
{
    [ExcludeFromCodeCoverage]
    public class NonstandardTests
    {
        TomlSettings GetSettings()
            => TomlSettings.Create(cfg => cfg.AllowNonstandard(true));

        [Fact]
        public void FakeJsonTest()
        {
            var toml = @"
{                 # This
  ""answer"": 42, # is
  answer2: 42     # not really
}                 # JSON
";
            var read = Toml.ReadString(toml, GetSettings());
            Assert.Equal(42, read["answer"].Get<int>());
            Assert.Equal(42, read["answer2"].Get<int>());
        }

        [Fact]
        public void InlineFakeJsonTest()
        {
            var toml = @"
json = {          # This
  ""answer"": 42, # is
  answer2: 42     # not really
}                 # JSON


# the example in README.md:

[global]
users = [
    {
        name = 'user1'
        group = 'foo'
        passwd = '123'
    },
    {
        ""name"": ""user2"",
        ""group"": ""bar"",
        ""passwd"": ""456""
    } # Yes, you can even paste a JSON here. XD
]
";
            var root = Toml.ReadString(toml, GetSettings());
            var json = root["json"] as TomlTable;
            Assert.Equal(42, json["answer"].Get<int>());
            Assert.Equal(42, json["answer2"].Get<int>());
            Assert.Equal("bar", root.Get<TomlTable>("global").Get<TomlTable[]>("users")[1].Get<string>("group"));
        }

        [Fact]
        public void InlineJsonWithoutCommasTest()
        {
            var toml = @"
json = {          # This
  answer: 42      # is
  ""answers"": [ 42 ] # not
  ""answers2"": [ 42 ]
  'answer2': 42     # really
}                 # JSON
";
            var json = Toml.ReadString(toml, GetSettings())["json"] as TomlTable;
            Assert.Equal(42, json["answer"].Get<int>());
            Assert.Equal(42, json["answers"].Get<int[]>()[0]);
            Assert.Equal(42, json["answer2"].Get<int>());
        }
    }
}
