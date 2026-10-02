using coworking.Models;
using Microsoft.EntityFrameworkCore;

namespace coworking.Data;

public class CoworkingDbContext(DbContextOptions<CoworkingDbContext> options) : DbContext(options)
{
    public DbSet<Room> Rooms => Set<Room>();

    public DbSet<Client> Clients => Set<Client>();

    public DbSet<Booking> Bookings => Set<Booking>();

    public DbSet<Service> Services => Set<Service>();

    public DbSet<BookingService> BookingServices => Set<BookingService>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Room>(entity =>
        {
            entity.ToTable("Rooms", table =>
            {
                table.HasCheckConstraint("CK_Rooms_Capacity", "[Capacity] > 0");
                table.HasCheckConstraint("CK_Rooms_HourlyRate", "[HourlyRate] >= 0");
            });
            entity.Property(room => room.Name).HasMaxLength(100).IsRequired();
            entity.Property(room => room.RoomType).HasMaxLength(50).IsRequired();
            entity.Property(room => room.HourlyRate).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("Clients");
            entity.Property(client => client.FullName).HasMaxLength(150).IsRequired();
            entity.Property(client => client.Email).HasMaxLength(200).IsRequired();
            entity.Property(client => client.Phone).HasMaxLength(30).IsRequired();
            entity.Property(client => client.Company).HasMaxLength(150);
            entity.HasIndex(client => client.Email).IsUnique();
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.ToTable("Bookings", table => table.HasCheckConstraint("CK_Bookings_EndAfterStart", "[EndTime] > [StartTime]"));
            entity.Property(booking => booking.Status).HasMaxLength(20).IsRequired();
            entity.HasIndex(booking => new { booking.RoomId, booking.StartTime });
            entity.HasOne(booking => booking.Room)
                .WithMany(room => room.Bookings)
                .HasForeignKey(booking => booking.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(booking => booking.Client)
                .WithMany(client => client.Bookings)
                .HasForeignKey(booking => booking.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.ToTable("Services", table => table.HasCheckConstraint("CK_Services_Price", "[Price] >= 0"));
            entity.Property(service => service.Name).HasMaxLength(100).IsRequired();
            entity.Property(service => service.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<BookingService>(entity =>
        {
            entity.ToTable("BookingServices", table => table.HasCheckConstraint("CK_BookingServices_Quantity", "[Quantity] > 0"));
            entity.HasIndex(line => new { line.BookingId, line.ServiceId }).IsUnique();
            entity.HasOne(line => line.Booking)
                .WithMany(booking => booking.BookingServices)
                .HasForeignKey(line => line.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(line => line.Service)
                .WithMany(service => service.BookingServices)
                .HasForeignKey(line => line.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
