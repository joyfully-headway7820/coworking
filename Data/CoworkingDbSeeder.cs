using coworking.Models;
using Microsoft.EntityFrameworkCore;

namespace coworking.Data;

public static class CoworkingDbSeeder
{
    public static async Task SeedAsync(CoworkingDbContext db)
    {
        if (await db.Rooms.AnyAsync())
            return;

        var alpha = Room("Переговорная Альфа", 6, 1500, "Переговорная", true);
        var beta = Room("Переговорная Бета", 8, 1800, "Переговорная", true);
        var open = Room("Опенспейс", 20, 700, "Опенспейс", true);
        var office12 = Room("Кабинет 12", 2, 1100, "Кабинет", true);
        var office14 = Room("Кабинет 14", 1, 900, "Кабинет", false);
        var hall = Room("Конференц-зал", 30, 3500, "Конференц-зал", true);
        var lounge = Room("Лаунж", 12, 600, "Лаунж", true);

        var ivan = Client("Иван Петров", "ivan.petrov@example.com", "+7 900 111-11-01", "ООО Север");
        var maria = Client("Мария Соколова", "maria.sokolova@example.com", "+7 900 111-11-02", null);
        var alexey = Client("Алексей Смирнов", "alexey.smirnov@example.com", "+7 900 111-11-03", "ИП Смирнов");
        var olga = Client("Ольга Кузнецова", "olga.kuznetsova@example.com", "+7 900 111-11-04", "АО Маяк");
        var dmitry = Client("Дмитрий Волков", "dmitry.volkov@example.com", "+7 900 111-11-05", null);
        var elena = Client("Елена Морозова", "elena.morozova@example.com", "+7 900 111-11-06", "ООО Поле");
        var sergey = Client("Сергей Новиков", "sergey.novikov@example.com", "+7 900 111-11-07", "Студия Норд");
        var anna = Client("Анна Лебедева", "anna.lebedeva@example.com", "+7 900 111-11-08", null);

        var coffee = Service("Кофе", 150, true);
        var projector = Service("Проектор", 500, true);
        var flipchart = Service("Флипчарт", 200, true);
        var water = Service("Вода", 80, true);
        var parking = Service("Парковка", 300, true);
        var print = Service("Печать", 15, false);

        var bookings = new List<Booking>
        {
            Book(alpha, ivan, 0, 10, 12, BookingStatuses.New, coffee, 2, projector, 1),
            Book(beta, maria, 0, 13, 15, BookingStatuses.Confirmed, water, 6),
            Book(open, alexey, 0, 9, 11, BookingStatuses.Completed, flipchart, 1),
            Book(hall, olga, 0, 16, 18, BookingStatuses.New, parking, 1, coffee, 1),
            Book(alpha, dmitry, 1, 10, 12, BookingStatuses.Confirmed, projector, 1),
            Book(beta, elena, 1, 10, 13, BookingStatuses.New, water, 2, coffee, 1),
            Book(office12, sergey, 2, 11, 12, BookingStatuses.New, flipchart, 1),
            Book(lounge, anna, 2, 15, 18, BookingStatuses.Confirmed, parking, 2),
            Book(open, ivan, -1, 10, 12, BookingStatuses.Completed, coffee, 1),
            Book(office12, maria, -2, 14, 16, BookingStatuses.Cancelled),
            Book(hall, dmitry, 3, 9, 13, BookingStatuses.Confirmed, projector, 1, water, 4),
            Book(alpha, anna, 3, 14, 16, BookingStatuses.New),
            Book(lounge, olga, -3, 12, 14, BookingStatuses.Cancelled, coffee, 1),
            Book(beta, alexey, 4, 9, 11, BookingStatuses.New, water, 1)
        };

        db.AddRange(alpha, beta, open, office12, office14, hall, lounge);
        db.AddRange(ivan, maria, alexey, olga, dmitry, elena, sergey, anna);
        db.AddRange(coffee, projector, flipchart, water, parking, print);
        db.AddRange(bookings);
        await db.SaveChangesAsync();
    }

    private static Room Room(string name, int capacity, decimal hourlyRate, string roomType, bool isActive) => new()
    {
        Name = name,
        Capacity = capacity,
        HourlyRate = hourlyRate,
        RoomType = roomType,
        IsActive = isActive
    };

    private static Client Client(string fullName, string email, string phone, string? company) => new()
    {
        FullName = fullName,
        Email = email,
        Phone = phone,
        Company = company
    };

    private static Service Service(string name, decimal price, bool isActive) => new()
    {
        Name = name,
        Price = price,
        IsActive = isActive
    };

    private static Booking Book(
        Room room,
        Client client,
        int dayOffset,
        int startHour,
        int endHour,
        string status,
        Service? first = null,
        int firstQuantity = 0,
        Service? second = null,
        int secondQuantity = 0)
    {
        var start = DateTime.Today.AddDays(dayOffset).AddHours(startHour);
        var booking = new Booking
        {
            Room = room,
            Client = client,
            StartTime = start,
            EndTime = start.AddHours(endHour - startHour),
            Status = status,
            CreatedAt = DateTime.Today.AddDays(dayOffset - 2).AddHours(9)
        };

        if (first is not null)
            booking.BookingServices.Add(new BookingService { Service = first, Quantity = firstQuantity });
        if (second is not null)
            booking.BookingServices.Add(new BookingService { Service = second, Quantity = secondQuantity });

        return booking;
    }
}
