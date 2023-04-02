using HotelAPI2.Common;

namespace HotelAPI2.Domain
{
	public class Client:AuditableBaseEntity
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Passport { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public string Age { get; set; } = "Adult";
		public virtual Reservation[] Reservations { get; set; }

	}
}
