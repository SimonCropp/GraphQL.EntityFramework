namespace SampleWeb.Client;

/// <summary>
/// The sample's default page: an ordinary Blazor page consuming the GraphQL API that
/// GraphQL.EntityFramework builds over EF Core. Each card is one of the things the library adds on
/// top of a plain schema — navigation properties resolved from the selection, <c>where</c> and
/// <c>orderBy</c> arguments turned into LINQ, and a Relay connection paged with cursors.
/// </summary>
public partial class Home
{
    // A header the sidecar can show; a real app would carry auth here.
    static readonly Dictionary<string, string> headers = new()
    {
        ["x-sample-app"] = "home"
    };

    const int pageSize = 2;

    List<Company>? companies;
    string? error;

    int minimumAge = 30;
    List<Employee>? filtered;
    bool filtering;

    Page? companyPage;

    // The "after" cursor each visited page started at. The last entry is the current page, so its
    // length is also the page number, and stepping back is a pop rather than a second query.
    readonly List<string?> starts = [null];

    // ReSharper disable once NotAccessedPositionalProperty.Local
    sealed record Company(int Id, string Content, List<Employee> Employees);

    // ReSharper disable once NotAccessedPositionalProperty.Local
    sealed record Employee(int Id, string Content, int Age);

    sealed record Page(int Total, bool HasNext, string? EndCursor, List<Company> Companies);

    protected override async Task OnInitializedAsync()
    {
        try
        {
            await LoadCompaniesAsync();
            await LoadFilteredAsync();
            await LoadPageAsync();
        }
        catch (Exception exception)
        {
            error = exception.Message;
        }
    }

    /// <summary>
    /// One request for companies and the employees of each. Nothing here declares a join: the
    /// nested selection is what tells the library to bring the navigation property along, so the
    /// shape of the query decides the shape of the database work.
    /// </summary>
    async Task LoadCompaniesAsync()
    {
        var document = await QueryAsync(
            """
            query Companies {
              companies(orderBy: id) {
                id
                content
                employees(orderBy: age_desc) {
                  id
                  content
                  age
                }
              }
            }
            """);

        companies =
        [
            .. Data(document)
                .GetProperty("companies")
                .EnumerateArray()
                .Select(_ => new Company(
                    _.GetProperty("id").GetInt32(),
                    Text(_, "content"),
                    [.. _.GetProperty("employees").EnumerateArray().Select(ToEmployee)]))
        ];
    }

    /// <summary>
    /// The <c>where</c> argument, which becomes a LINQ predicate against the database rather than a
    /// filter over results already fetched — so the rows below the threshold are never read.
    /// </summary>
    async Task LoadFilteredAsync()
    {
        filtering = true;
        try
        {
            var document = await QueryAsync(
                """
                query OlderThan($age: Int) {
                  employees(
                    where: {age: {greaterThanOrEqual: $age}}
                    orderBy: age_desc) {
                    id
                    content
                    age
                  }
                }
                """,
                JsonSerializer.SerializeToElement(new {age = minimumAge}));

            filtered = [.. Data(document).GetProperty("employees").EnumerateArray().Select(ToEmployee)];
        }
        finally
        {
            filtering = false;
        }
    }

    /// <summary>
    /// A Relay connection: totalCount is a second aggregate over the same query, and the cursor is
    /// what the next page resumes from — skip/take against the database, not a paged list in memory.
    /// </summary>
    async Task LoadPageAsync()
    {
        var document = await QueryAsync(
            """
            query CompanyPage($first: Int, $after: String) {
              companiesConnection(first: $first, after: $after) {
                totalCount
                pageInfo {
                  hasNextPage
                  endCursor
                }
                items {
                  id
                  content
                }
              }
            }
            """,
            JsonSerializer.SerializeToElement(new {first = pageSize, after = starts[^1]}));

        var connection = Data(document).GetProperty("companiesConnection");
        var info = connection.GetProperty("pageInfo");
        companyPage = new(
            connection.GetProperty("totalCount").GetInt32(),
            info.GetProperty("hasNextPage").GetBoolean(),
            info.GetProperty("endCursor").GetString(),
            [
                .. connection
                    .GetProperty("items")
                    .EnumerateArray()
                    .Select(_ => new Company(_.GetProperty("id").GetInt32(), Text(_, "content"), []))
            ]);
    }

    Task NextPageAsync()
    {
        if (companyPage?.EndCursor is not {} cursor)
        {
            return Task.CompletedTask;
        }

        starts.Add(cursor);
        return LoadPageAsync();
    }

    Task PreviousPageAsync()
    {
        if (starts.Count == 1)
        {
            return Task.CompletedTask;
        }

        starts.RemoveAt(starts.Count - 1);
        return LoadPageAsync();
    }

    static Employee ToEmployee(JsonElement element) =>
        new(
            element.GetProperty("id").GetInt32(),
            Text(element, "content"),
            element.GetProperty("age").GetInt32());

    // Content is nullable on the entity, so a row that never set one is not an error.
    static string Text(JsonElement element, string name) =>
        element.GetProperty(name).GetString() ?? "";

    /// <summary>
    /// Surfaces a GraphQL error document as an exception. Without this a failed query reads as an
    /// empty card, which looks like a schema that returned nothing.
    /// </summary>
    static JsonElement Data(JsonElement document)
    {
        if (document.TryGetProperty("errors", out var errors) &&
            errors.GetArrayLength() > 0)
        {
            throw new(Text(errors[0], "message"));
        }

        return document.GetProperty("data");
    }

    // The fetcher shape is a stream of documents; a query is the one-document case.
    async Task<JsonElement> QueryAsync(string query, JsonElement? variables = null)
    {
        await foreach (var document in Fetcher.FetchAsync(new(query, variables), headers, Cancel.None))
        {
            return document;
        }

        throw new("The endpoint returned no document.");
    }
}
