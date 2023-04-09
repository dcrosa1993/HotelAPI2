using HotelAPI2.Common;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace HotelAPI2.Domain
{
	public class Room:AuditableBaseEntity
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public string Number { get; set; }
		public int Capacity { get; set; }
		public bool Availability { get; set; } = true;
		[AllowNull]
		public int ReservationId { get; set; }
		public virtual Reservation Reservation { get; set; }
	}
}
