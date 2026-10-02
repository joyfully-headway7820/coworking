namespace coworking.App_Start;

public static class RouteConfig
{
    public static void Map(WebApplication app)
    {
        app.MapControllers();
        app.MapGet("/", () => Results.Redirect("/catalog/rooms"));
        app.MapGet("/error", () => Results.Problem("Внутренняя ошибка сервера."));

        app.MapControllerRoute(
            name: "catalog-rooms",
            pattern: "catalog/rooms",
            defaults: new { controller = "Rooms", action = "Index" });

        app.MapControllerRoute(
            name: "room-details",
            pattern: "room/{id:int}",
            defaults: new { controller = "Rooms", action = "Details" });

        app.MapControllerRoute(
            name: "client-bookings",
            pattern: "client/{id:int}/bookings",
            defaults: new { controller = "Clients", action = "Bookings" });

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Rooms}/{action=Index}/{id?}");
    }
}
