using HotelAPI2.Common;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HotelAPI2.Domain
{
	public class ReservationConfiguration:AuditableBaseEntity
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public int AdvanceRequieredNigth { get; set; }
		public int NigthCostUnder2Client { get; set; }
		public int NigthCostFor3Client { get; set; }
		public int NigthCostMost4Client { get; set; }
		public int NigthDiscountUnder2Client { get; set; }
		public int NigthDiscountFor3Client { get; set; }
		public int NigthDiscountMost4Client { get; set; }
		public int MaxRoomClients { get; set; }
		public int TransportCost { get; set; }
		public int ManagerPartPerClient { get; set; }

	}
}
