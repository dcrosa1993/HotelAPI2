using HotelAPI2.Common;
using HotelAPI2.Domain;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace HotelAPI2.DTOs
{
	public class ReservationsOutput : AuditableBaseEntity
	{
		public int Id { get; set; }
		public int NoClients { get; set; }
		public string Description { get; set; } = string.Empty;
		public DateTime DateIn { get; set; }
		public DateTime DateOut { get; set; }
		public double CostPerClient { get; set; }
		public double Discount { get; set; }
		public int TotalNights { get; set; }
		public int PaymentNights { get; set; }
		public double Management { get; set; }
		public double Transport { get; set; }
		public bool AdvanceManagement { get; set; } = false;
		public string UserEmail { get; set; } = string.Empty;
		public int? RoomId { get; set; }
		public double LodgingCostPerClient { get; set; }
		public double TotalCostPerClient { get; set; }
		public double TotalPaydedPerClient { get; set; }
		public double PendingPerClient { get; set; }
		public double TotalPayded { get; set; }
		public double TotalPending { get; set; }
		public double TotalCost { get; set; }
		public ICollection<ClientOutput> Clients { get; set; } = new List<ClientOutput>();
		public DateTime CreatedTime { get; set; }
		public DateTime? UpdatedTime { get; set; }
		public string CreatedBy { get; set; }
		public string UpdatedBy { get; set; }
	}
}
