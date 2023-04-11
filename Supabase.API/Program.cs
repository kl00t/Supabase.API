using Supabase;
using Supabase.API.Contracts;
using Supabase.API.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped(_ =>
    new Client(
        builder.Configuration["SupabaseUrl"],
        builder.Configuration["SupabaseKey"],
        new SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = true,
        }));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/newsletters", async (
    CreateNewsletterRequest request,
    Client client) =>
{
    var newsletter = new Newsletter
    {
        Name = request.Name,
        Description = request.Description,
        ReadTime = request.ReadTime
    };

    var response = await client.From<Newsletter>().Insert(newsletter);

    var newNewsletter = response.Models.First();

    return Results.Ok(newNewsletter.Id);
});

app.MapGet("/newsletters/{id}", async (long id, Client client) =>
{
    var response = await client
        .From<Newsletter>()
        .Where(n => n.Id == id)
        .Get();

    var newsletter = response.Models.FirstOrDefault();
    if (newsletter is null)
    {
        return Results.NotFound();
    }

    var newsletterResponse = new NewsletterResponse
    {
        Id = newsletter.Id,
        Name = newsletter.Name,
        Description = newsletter.Description,
        ReadTime = newsletter.ReadTime,
        CreatedAt = newsletter.CreatedAt,
    };

    return Results.Ok(newsletterResponse);
});


app.MapDelete("/newsletters/{id}", async (long id, Client client) =>
{
    await client.From<Newsletter>().Where(n => n.Id == id).Delete();
    return Results.NoContent();
});

app.UseHttpsRedirection();

app.Run();