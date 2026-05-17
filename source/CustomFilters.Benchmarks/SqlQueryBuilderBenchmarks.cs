using BenchmarkDotNet.Attributes;
using CustomFilters.TagManager;

namespace CustomFilters.Benchmarks;

[MemoryDiagnoser]
public class SqlQueryBuilderBenchmarks
{
    private static readonly string[] OneTags = ["assault"];
    private static readonly string[] TenTags =
        ["assault", "ecm", "uac", "lrm", "srm", "active_probe", "narc", "tag", "ams", "guardian"];
    private static readonly string[] TagsWithSpecialChars = ["it's", "50%_off", "underscored_tag"];

    // Quote

    [Benchmark]
    public string Quote_Plain() => SqlQueryBuilder.Quote("assault");

    [Benchmark]
    public string Quote_WithSingleQuote() => SqlQueryBuilder.Quote("it's");

    // QuoteLike

    [Benchmark]
    public string QuoteLike_Plain() => SqlQueryBuilder.QuoteLike("mech");

    [Benchmark]
    public string QuoteLike_WithWildcardChars() => SqlQueryBuilder.QuoteLike("50%_off");

    // ExistsIn

    [Benchmark]
    public string ExistsIn_OneTag() => SqlQueryBuilder.ExistsIn(OneTags);

    [Benchmark]
    public string ExistsIn_TenTags() => SqlQueryBuilder.ExistsIn(TenTags);

    // NotExistsIn

    [Benchmark]
    public string NotExistsIn_TenTags() => SqlQueryBuilder.NotExistsIn(TenTags);

    // ExistsLike

    [Benchmark]
    public string ExistsLike_Plain() => SqlQueryBuilder.ExistsLike("mech");

    [Benchmark]
    public string ExistsLike_WithSpecialChars() => SqlQueryBuilder.ExistsLike("50%_off");

    // JoinAndAddIfNotEmpty

    [Benchmark]
    public int JoinAndAddIfNotEmpty_NonEmpty()
    {
        var outer = new List<string>();
        var inner = new List<string>(TenTags);
        SqlQueryBuilder.JoinAndAddIfNotEmpty(outer, " AND ", inner);
        return outer.Count;
    }

    [Benchmark]
    public int JoinAndAddIfNotEmpty_Empty()
    {
        var outer = new List<string>();
        var inner = new List<string>();
        SqlQueryBuilder.JoinAndAddIfNotEmpty(outer, " AND ", inner);
        return outer.Count;
    }

    // Full filter build (realistic use: combine multiple clauses)

    [Benchmark]
    public string FullFilter_TenIncludes_TwoLikes()
    {
        var outer = new List<string>();
        var includes = TenTags.Select(t => SqlQueryBuilder.ExistsIn(t)).ToList();
        SqlQueryBuilder.JoinAndAddIfNotEmpty(outer, " OR ", includes);
        outer.Add(SqlQueryBuilder.ExistsLike("active"));
        outer.Add(SqlQueryBuilder.ExistsLike("guardian"));
        return string.Join(" AND ", outer);
    }
}
