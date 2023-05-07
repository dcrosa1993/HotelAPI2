
using HotelAPI2.Domain;
using System.Diagnostics.CodeAnalysis;

namespace HotelAPI2.DTOs
{
	public class ReservationsInput
	{
		public int NoClients { get; set; }
		public string Description { get; set; } = string.Empty;
		public DateTime DateIn { get; set; }
		public DateTime DateOut { get; set; }
		public int PaymentNights { get; set; }
		public bool AdvanceManagement { get; set; } = false;
		public ICollection<ClientInput> Clients { get; set; } = new List<ClientInput>();

	}
}
