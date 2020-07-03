using System;
using System.Collections.Generic;
using WebCrawler;
using Xunit;

namespace Test.Extensions
{
    public class Test_StringExtensions
    {
        [Fact]
        public void TestClean()
        {
            var str = "\n\t\r-'s!&$%";
            str = str.Clean();

            Assert.Equal(4, str.Length);

            str = str.Clean(true);

            Assert.Empty(str);

            str = "a1b2**%^$";
            str = str.Clean();

            Assert.Equal(4, str.Length);

            str = "";
            str = str.Clean();

            Assert.Empty(str);

            str = null;
            str = str.Clean();

            Assert.Empty(str);
        }

        [Fact]
        public void TestIsValidWord()
        {
            var str = "fdsadfsa";

            Assert.True(str.IsValidWord());

            str = "abc%d";

            Assert.False(str.IsValidWord());

            str = "abdce f";

            Assert.False(str.IsValidWord());

            str = "ab4";

            Assert.False(str.IsValidWord());

            str = "abcd.";

            Assert.False(str.IsValidWord());

            str = "fdksalfjdslafjdkslafjdslafjdlksa";

            Assert.True(str.IsValidWord());

            str = "";

            Assert.False(str.IsValidWord());

            str = null;

            Assert.False(str.IsValidWord());
        }

        [Fact]
        public void TestTokenizeToList()
        {
            var str  = "this,that,them,they";
            var list = str.TokenizeToList(",");

            Assert.Equal(new List<string> { "this", "that", "them", "they" }, list);

            str  = "this that them they";
            list = str.TokenizeToList();

            Assert.Equal(new List<string> { "this", "that", "them", "they" }, list);

            str = "this that them they,";
            list = str.TokenizeToList();

            Assert.Equal(new List<string> { "this", "that", "them", "they," }, list);

            str  = "";
            list = str.TokenizeToList();

            Assert.Empty(list);

            str  = null;
            list = str.TokenizeToList();

            Assert.Empty(list);
        }
    }
}
