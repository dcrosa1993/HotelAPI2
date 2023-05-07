using HotelAPI2.Common;
using HotelAPI2.Domain;

namespace HotelAPI2.DTOs
{
	public class ClientOutput:AuditableBaseEntity
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Passport { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public string Age { get; set; } = "adult";
		public ICollection<Reservation>? Reservations { get; set; } 
	}
}
