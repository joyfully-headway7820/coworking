using coworking.Data;
using coworking.Models;
using coworking.Mvc;
using coworking.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace coworking.Controllers;

public class ClientsController(CoworkingDbContext db) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var clients = await db.Clients.AsNoTracking()
            .OrderBy(client => client.FullName)
            .Select(client => new ClientRowViewModel
            {
                Id = client.Id,
                FullName = client.FullName,
                Email = client.Email,
                Phone = client.Phone,
                Company = client.Company,
                BookingCount = client.Bookings.Count
            })
            .ToListAsync(cancellationToken);

        return View(clients);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var client = await Load(id, cancellationToken);
        return client is null ? this.Missing() : View(client);
    }

    public async Task<IActionResult> Bookings(int id, CancellationToken cancellationToken)
    {
        var client = await Load(id, cancellationToken);
        return client is null ? this.Missing() : View(client);
    }

    public IActionResult Create() => View(new ClientFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClientFormViewModel model, CancellationToken cancellationToken)
    {
        await ValidateEmail(model, null, cancellationToken);
        if (!ModelState.IsValid)
            return View(model);

        var client = new Client();
        Apply(client, model);
        db.Clients.Add(client);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Message"] = "Клиент создан.";
        return RedirectToAction(nameof(Details), new { id = client.Id });
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await db.Clients.AsNoTracking()
            .Where(client => client.Id == id)
            .Select(client => new ClientFormViewModel
            {
                Id = client.Id,
                FullName = client.FullName,
                Email = client.Email,
                Phone = client.Phone,
                Company = client.Company
            })
            .FirstOrDefaultAsync(cancellationToken);

        return model is null ? this.Missing() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClientFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
            return this.Missing();

        await ValidateEmail(model, id, cancellationToken);
        if (!ModelState.IsValid)
            return View(model);

        var client = await db.Clients.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (client is null)
            return this.Missing();

        Apply(client, model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Message"] = "Клиент сохранён.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<ClientDetailsViewModel?> Load(int id, CancellationToken cancellationToken)
    {
        return await db.Clients.AsNoTracking()
            .Where(client => client.Id == id)
            .Select(client => new ClientDetailsViewModel
            {
                Id = client.Id,
                FullName = client.FullName,
                Email = client.Email,
                Phone = client.Phone,
                Company = client.Company,
                Bookings = client.Bookings
                    .OrderByDescending(booking => booking.StartTime)
                    .Select(booking => new BookingLinkRowViewModel
                    {
                        Id = booking.Id,
                        RoomId = booking.RoomId,
                        RoomName = booking.Room.Name,
                        ClientId = client.Id,
                        ClientName = client.FullName,
                        StartTime = booking.StartTime,
                        EndTime = booking.EndTime,
                        Status = booking.Status
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task ValidateEmail(ClientFormViewModel model, int? currentId, CancellationToken cancellationToken)
    {
        var email = model.Email?.Trim() ?? "";
        var taken = await db.Clients.AnyAsync(client =>
            client.Email == email && (currentId == null || client.Id != currentId), cancellationToken);
        if (taken)
            ModelState.AddModelError(nameof(model.Email), "Клиент с такой почтой уже есть.");
    }

    private static void Apply(Client client, ClientFormViewModel model)
    {
        client.FullName = model.FullName.Trim();
        client.Email = model.Email.Trim();
        client.Phone = model.Phone.Trim();
        client.Company = string.IsNullOrWhiteSpace(model.Company) ? null : model.Company.Trim();
    }
}
