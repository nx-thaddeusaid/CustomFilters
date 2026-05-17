using System.Collections.Generic;
using CustomFilters.TagManager;
using Xunit;

namespace CustomFilters.Tests;

public class SqlQueryBuilderTests
{
    // Quote

    [Fact]
    public void Quote_PlainString_WrapsInSingleQuotes()
    {
        Assert.Equal("'hello'", SqlQueryBuilder.Quote("hello"));
    }

    [Fact]
    public void Quote_StringWithSingleQuote_EscapesIt()
    {
        Assert.Equal("'it''s'", SqlQueryBuilder.Quote("it's"));
    }

    [Fact]
    public void Quote_EmptyString_ReturnsEmptyQuoted()
    {
        Assert.Equal("''", SqlQueryBuilder.Quote(""));
    }

    [Fact]
    public void Quote_MultipleSingleQuotes_EscapesAll()
    {
        Assert.Equal("'a''b''c'", SqlQueryBuilder.Quote("a'b'c"));
    }

    // QuoteLike

    [Fact]
    public void QuoteLike_PlainTerm_WrapsWithWildcards()
    {
        Assert.Equal("'%hello%'", SqlQueryBuilder.QuoteLike("hello"));
    }

    [Fact]
    public void QuoteLike_TermWithPercent_EscapesPercent()
    {
        Assert.Equal("'%100\\%%'", SqlQueryBuilder.QuoteLike("100%"));
    }

    [Fact]
    public void QuoteLike_TermWithUnderscore_EscapesUnderscore()
    {
        Assert.Equal("'%unit\\_light%'", SqlQueryBuilder.QuoteLike("unit_light"));
    }

    [Fact]
    public void QuoteLike_TermWithSingleQuote_EscapesBoth()
    {
        Assert.Equal("'%it''s%'", SqlQueryBuilder.QuoteLike("it's"));
    }

    // ExistsIn

    [Fact]
    public void ExistsIn_SingleTag_BuildsCorrectSubquery()
    {
        var result = SqlQueryBuilder.ExistsIn("unit_light");
        Assert.Contains("IN ('unit_light')", result);
        Assert.StartsWith("EXISTS", result);
    }

    [Fact]
    public void ExistsIn_MultipleTags_CommaSepaerated()
    {
        var result = SqlQueryBuilder.ExistsIn("unit_light", "unit_medium");
        Assert.Contains("IN ('unit_light','unit_medium')", result);
    }

    [Fact]
    public void ExistsIn_TagWithSingleQuote_EscapedInQuery()
    {
        var result = SqlQueryBuilder.ExistsIn("it's");
        Assert.Contains("IN ('it''s')", result);
    }

    // NotExistsIn

    [Fact]
    public void NotExistsIn_SingleTag_PrependsNot()
    {
        var result = SqlQueryBuilder.NotExistsIn("unit_blacklisted");
        Assert.StartsWith("NOT EXISTS", result);
        Assert.Contains("'unit_blacklisted'", result);
    }

    // ExistsLike

    [Fact]
    public void ExistsLike_PlainTerm_BuildsLikeSubquery()
    {
        var result = SqlQueryBuilder.ExistsLike("light");
        Assert.Contains("LIKE '%light%'", result);
        Assert.StartsWith("EXISTS", result);
    }

    [Fact]
    public void ExistsLike_TermWithUnderscore_EscapedInPattern()
    {
        var result = SqlQueryBuilder.ExistsLike("unit_light");
        Assert.Contains(@"LIKE '%unit\_light%'", result);
    }

    [Fact]
    public void ExistsLike_NegatedTerm_CallerPrependsNot()
    {
        // ExistsLike itself doesn't negate; callers prepend "NOT " for !term
        var result = "NOT " + SqlQueryBuilder.ExistsLike("blacklisted");
        Assert.StartsWith("NOT EXISTS", result);
    }

    // JoinAndAddIfNotEmpty

    [Fact]
    public void JoinAndAddIfNotEmpty_NonEmptyInner_AddsToOuter()
    {
        var outer = new List<string>();
        var inner = new List<string> { "a", "b" };
        SqlQueryBuilder.JoinAndAddIfNotEmpty(outer, " OR ", inner);
        Assert.Single(outer);
        Assert.Equal("(a OR b)", outer[0]);
    }

    [Fact]
    public void JoinAndAddIfNotEmpty_EmptyInner_OuterUnchanged()
    {
        var outer = new List<string> { "existing" };
        SqlQueryBuilder.JoinAndAddIfNotEmpty(outer, " OR ", new List<string>());
        Assert.Single(outer);
        Assert.Equal("existing", outer[0]);
    }

    [Fact]
    public void JoinAndAddIfNotEmpty_SingleInnerItem_NoBracketedJoin()
    {
        var outer = new List<string>();
        SqlQueryBuilder.JoinAndAddIfNotEmpty(outer, " AND ", new List<string> { "x" });
        Assert.Equal("(x)", outer[0]);
    }
}
