var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// "Open in BlazorQL" on a captured request opens the query explorer page.
builder.Services.AddBlazorQLSidecar(_ => _.IdeRoute = "explorer");

// Unlike a schema that runs in the browser, this one is the host app's: every request leaves as an
// http post to /graphql, where GraphQL.EntityFramework turns it into a database query. Both pages
// resolve this one fetcher, so the sidecar records the home page's requests and the explorer's
// alike. HttpFetcher's own HttpClient has no base address, and a WebAssembly app has no ambient
// one, so the origin the app was served from is what makes the relative endpoint resolve.
builder.Services.AddSingleton<IGraphQLFetcher>(
    _ => new SidecarFetcher(
        new HttpFetcher(
            new()
            {
                BaseAddress = new(builder.HostEnvironment.BaseAddress)
            },
            "graphql"),
        _.GetRequiredService<SidecarStore>()));

await builder.Build().RunAsync();
