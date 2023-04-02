using HotelAPI2.Common;

namespace HotelAPI2.Domain
{
	public class Room:AuditableBaseEntity
	{
		public int Id { get; set; }
		public string Number { get; set; }
		public int Capacity { get; set; }
		public bool Availability { get; set; } = true;
		public int ReservationId { get; set; }
		public virtual Reservation Reservation { get; set; }
	}
}
