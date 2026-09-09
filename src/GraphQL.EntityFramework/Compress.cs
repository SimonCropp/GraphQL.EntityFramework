namespace GraphQL.EntityFramework;

public static class Compress
{
    /// <summary>
    /// Strip insignificant whitespace from a GraphQL query.
    /// </summary>
    /// <remarks>
    /// Scans rather than pattern matches, because whitespace is only insignificant outside string
    /// literals. A regex that collapses runs of whitespace and strips it around punctuators also
    /// rewrites the contents of any literal that contains them, so `value: "Smith, John"` silently
    /// became `value:"Smith,John"`. The query still parses, so the corruption shows up as querying
    /// for the wrong data rather than as an error.
    /// </remarks>
    public static string Query(string query)
    {
        Ensure.NotWhiteSpace(nameof(query), query);

        var builder = new StringBuilder(query.Length);
        var pendingSpace = false;
        var lastWasPunctuator = false;
        var index = 0;

        while (index < query.Length)
        {
            var current = query[index];

            if (char.IsWhiteSpace(current))
            {
                pendingSpace = true;
                index++;
                continue;
            }

            // A comment runs to the end of the line and carries no meaning. It has to be dropped
            // rather than collapsed, since turning the newline into a space would pull the rest of
            // the query into the comment.
            if (current == '#')
            {
                while (index < query.Length &&
                       query[index] is not ('\n' or '\r'))
                {
                    index++;
                }

                pendingSpace = true;
                continue;
            }

            var isPunctuator = IsPunctuator(current);

            // Whitespace is only needed to keep two names apart
            if (pendingSpace &&
                builder.Length > 0 &&
                !lastWasPunctuator &&
                !isPunctuator)
            {
                builder.Append(' ');
            }

            pendingSpace = false;

            if (current == '"')
            {
                index = AppendStringValue(query, index, builder);
                lastWasPunctuator = false;
                continue;
            }

            builder.Append(current);
            lastWasPunctuator = isPunctuator;
            index++;
        }

        return builder.ToString();
    }

    static bool IsPunctuator(char character) =>
        character is '[' or ']' or '{' or '}' or '(' or ')' or ':' or ',';

    /// <summary>
    /// Copy a string literal through untouched, and return the index just past it.
    /// </summary>
    static int AppendStringValue(string query, int index, StringBuilder builder)
    {
        if (IsBlockStringDelimiter(query, index))
        {
            var end = query.IndexOf("\"\"\"", index + 3, StringComparison.Ordinal);
            if (end == -1)
            {
                // Unterminated. Nothing sensible to compress, so pass the remainder through and let
                // the server report the syntax error.
                builder.Append(query, index, query.Length - index);
                return query.Length;
            }

            var length = end + 3 - index;
            builder.Append(query, index, length);
            return index + length;
        }

        builder.Append('"');
        index++;

        while (index < query.Length)
        {
            var current = query[index];
            builder.Append(current);
            index++;

            if (current == '\\')
            {
                // The escaped character is part of the literal, so a `\"` does not end it
                if (index < query.Length)
                {
                    builder.Append(query[index]);
                    index++;
                }

                continue;
            }

            if (current == '"')
            {
                break;
            }
        }

        return index;
    }

    static bool IsBlockStringDelimiter(string query, int index) =>
        index + 2 < query.Length &&
        query[index + 1] == '"' &&
        query[index + 2] == '"';
}
