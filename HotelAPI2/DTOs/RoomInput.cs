using HotelAPI2.Domain;
using System.Diagnostics.CodeAnalysis;

namespace HotelAPI2.DTOs
{
	public class RoomInput
	{
		public string Number { get; set; }
		public int Capacity { get; set; }
		public bool Availability { get; set; } = true;
	}
}
