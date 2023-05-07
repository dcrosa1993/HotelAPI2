using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace HotelAPI2.Domain
{
	public class HotelContext : DbContext
	{
		public HotelContext(DbContextOptions<HotelContext> options) : base(options)
		{
			
		}

		public DbSet<User> Users { get; set; }
		public DbSet<Client> Clients { get; set; }
		public DbSet<Room> Rooms { get; set; }
		public DbSet<Reservation> Reservations { get; set; }
		public DbSet<ReservationConfiguration> ReservationConfiguration { get; set; }
		
		public DbSet<UserAccess> UserAccesses { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<User>().HasData(new User {
				Id= -1,
				Baned= false,
				ChangePassword= false,
				CreatedBy= "admin@realtravelservices.com",
				CreatedTime= DateTime.UtcNow,
				Email = "admin@realtravelservices.com",
				Name = "Admin Hotel Real",
				Password = "RealTravel2023",
				Phone = "00000000",
				Reservations= new List<Reservation>() { },
				Role = "admin",
				UpdatedBy = "admin@realtravelservices.com",
				UpdatedTime= DateTime.UtcNow,
			});

			modelBuilder.Entity<ReservationConfiguration>().HasData(new ReservationConfiguration
			{
				Id= -1,
				AdvanceRequieredNigth = 0,
				NigthCostUnder2Client = 0,
				NigthCostFor3Client = 0,
				NigthCostMost4Client = 0,
				NigthDiscountUnder2Client = 0,
				NigthDiscountFor3Client = 0,
				NigthDiscountMost4Client = 0,
				MaxRoomClients = 0,
				TransportCost = 0,
				ManagerPartPerClient = 0,
				CreatedBy = "admin@realtravelservices.com",
				CreatedTime = DateTime.UtcNow,
				UpdatedBy = "admin@realtravelservices.com",
				UpdatedTime = DateTime.UtcNow,
			});
			base.OnModelCreating(modelBuilder);
		}
		}
	}
