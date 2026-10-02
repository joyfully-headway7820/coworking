using System.Globalization;
using coworking.App_Start;
using coworking.Data;
using coworking.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<CoworkingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Coworking")));
builder.Services.AddScoped<IBookingPriceService, DefaultBookingPriceService>();
builder.Services.AddScoped<IBookingSchedule, BookingSchedule>();
builder.Services.AddScoped<BookingFormPrep>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseExceptionHandler("/error");

var invariant = CultureInfo.InvariantCulture;
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(invariant),
    SupportedCultures = [invariant],
    SupportedUICultures = [invariant]
});

app.UseStaticFiles();
app.UseRouting();
RouteConfig.Map(app);

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CoworkingDbContext>();
    await db.Database.MigrateAsync();
    await CoworkingDbSeeder.SeedAsync(db);
}

app.Run();
