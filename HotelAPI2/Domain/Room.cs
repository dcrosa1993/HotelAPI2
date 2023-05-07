using HotelAPI2.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelAPI2.Domain
{
	public class Room:AuditableBaseEntity
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public string Number { get; set; } = string.Empty;
		public int Capacity { get; set; }
		public bool Availability { get; set; } = true;
		public int? ReservationId { get; set; }
		
	}
}
