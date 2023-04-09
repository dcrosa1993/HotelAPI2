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
		public string Name { get; set; }
		public string Passport { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public string Age { get; set; } = "Adult";
		public virtual ICollection<Reservation> Reservations { get; set; }

	}
}
