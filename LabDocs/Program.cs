using Ganss.Xss;
using LabDocs.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSingleton<HtmlSanitizer>();
builder.Services.AddScoped<WikiPageService>();
builder.Services.AddScoped<WikiCatalogService>();



var app = builder.Build();

string wikiContentPath = WikiContentDirectory.GetPath(app.Environment);
WikiContentDirectory.EnsureExists(wikiContentPath);

using (IServiceScope validationScope = app.Services.CreateScope())
{
    WikiCatalogService catalogService =
        validationScope.ServiceProvider.GetRequiredService<WikiCatalogService>();

    catalogService.GetPages();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found");

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

