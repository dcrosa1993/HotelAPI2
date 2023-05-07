using HotelAPI2.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelAPI2.Domain
{
	public class Client:AuditableBaseEntity
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Passport { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Phone { get; set; } = string.Empty;
		public string Age { get; set; } = "adult";
		public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

	}
}
