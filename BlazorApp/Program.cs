using BlazorApp.Data;
using BlazorApp.Hubs;
using BlazorApp.Services;
using Marten;
using Marten.Events.Daemon.Resiliency;
using Marten.Events.Projections;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Weasel.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddScoped<QuestService>();
builder.Services.AddScoped<ICDService>();
builder.Services.AddSingleton<IMartenEventsConsumer, LOTREventsConsumer>();

builder.Services.AddMarten(provider =>
{
    var options = new StoreOptions();

    // Establish the connection string to your Marten database
    options.Connection(builder.Configuration.GetConnectionString("Marten"));

    // If we're running in development mode, let Marten just take care
    // of all necessary schema building and patching behind the scenes
    if (builder.Environment.IsDevelopment())
    {
        options.AutoCreateSchemaObjects = AutoCreate.All;
    }

    // Register any projections you need to run asynchronously
    //options.Projections.Add<TripAggregationWithCustomName>(ProjectionLifecycle.Async);
    options.Projections.Add(
        new LOTRSubscription(provider.GetService<IMartenEventsConsumer>()),
        ProjectionLifecycle.Async,
        "lotrConsumer"
    );


    options.Schema.For<LOTRShared.Domain.ICDRecord>()
        .Index(x => x.Code)
        .NgramIndex(x => x.Description);

    return options;
})
// Turn on the async daemon in "Solo" mode
.AddAsyncDaemon(DaemonMode.Solo); ;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();

app.MapHub<QuestHub>("/questhub");
app.MapFallbackToPage("/_Host");

app.Run();
