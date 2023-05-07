using HotelAPI2.Common;
using HotelAPI2.Domain;
using System.Diagnostics.CodeAnalysis;

namespace HotelAPI2.DTOs
{
	public class RoomOutput : AuditableBaseEntity
	{
		public int Id { get; set; }
		public string Number { get; set; } = string.Empty;
		public int Capacity { get; set; }
		public bool Availability { get; set; }
		public int? ReservationId { get; set; }
	}
}
