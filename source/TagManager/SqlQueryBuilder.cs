using System.Collections.Generic;
using System.Linq;

namespace CustomFilters.TagManager;

// Pure SQL string-building helpers, extracted from FilterQueries for testability.
internal static class SqlQueryBuilder
{
    internal static string Quote(string text)
    {
        return "'" + text.Replace(@"'", @"''") + "'";
    }

    internal static string QuoteLike(string text)
    {
        return Quote('%' + text.Replace(@"%", @"\%").Replace(@"_", @"\_") + '%');
    }

    internal static string ExistsIn(params string[] tags)
    {
        var quotedTags = string.Join(",", tags.Select(Quote));
        return @$"EXISTS ( SELECT * FROM TagSetTag t WHERE d.TagSetID = t.TagSetID AND t.TagName IN ({quotedTags}) )";
    }

    internal static string NotExistsIn(params string[] tags)
    {
        return "NOT " + ExistsIn(tags);
    }

    internal static string ExistsLike(string term)
    {
        return @$"EXISTS ( SELECT * FROM TagSetTag t WHERE d.TagSetID = t.TagSetID AND t.TagName LIKE {QuoteLike(term)} ESCAPE '\' )";
    }

    internal static void JoinAndAddIfNotEmpty(List<string> outer, string separator, List<string> inner)
    {
        if (inner.Count > 0)
        {
            outer.Add("(" + string.Join(separator, inner) + ")");
        }
    }
}
