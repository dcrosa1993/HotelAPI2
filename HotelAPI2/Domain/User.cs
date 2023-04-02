using HotelAPI2.Common;

namespace HotelAPI2.Domain
{
	public class User:AuditableBaseEntity
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public string Phone { get; set; }
		public string[] Role { get; set; }
		public bool Baned { get; set; } = false;
		public string[] Access { get; set; }
		public bool ChangePassword { get; set; } = false;
		public virtual Reservation[] Reservations { get; set; }
	}
}
