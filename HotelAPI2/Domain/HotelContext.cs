using Microsoft.EntityFrameworkCore;

namespace HotelAPI2.Domain
{
	public class HotelContext : DbContext
	{
		public HotelContext(DbContextOptions<HotelContext> options) : base(options)
		{
			
		}

		public DbSet<User> Users { get; set; }
		public DbSet<Client> Clients { get; set; }
		public DbSet<Reservation> Reservations { get; set; }
		public DbSet<ReservationConfiguration> ReservationsConfiguration { get; set; }
		public DbSet<Room> Rooms { get; set; }
		public DbSet<UserAccess> UserAccesses { get; set; }
	}
}
