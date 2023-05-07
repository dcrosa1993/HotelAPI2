using HotelAPI2.Common;
using HotelAPI2.Domain;

namespace HotelAPI2.DTOs
{
	public class UserOutput : AuditableBaseEntity
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Phone { get; set; } = string.Empty;
		public string Role { get; set; } = string.Empty;
		public bool Baned { get; set; } = false;
		public ICollection<UserAccess>? UserAccess { get; set; }
		public bool ChangePassword { get; set; } = false;
		public ICollection<ReservationsOutput>? Reservations { get; set; }
	}
}
