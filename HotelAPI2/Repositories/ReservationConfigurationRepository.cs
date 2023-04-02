using HotelAPI2.Domain;
using HotelAPI2.DTOs;

namespace HotelAPI2.Repositories
{
	public class ReservationConfigurationRepository
	{
		private ReservationConfiguration item = new ReservationConfiguration()
		{
			Id = 1,
			CreatedBy = null,
			CreatedTime = DateTime.UtcNow,
			UpdatedTime = DateTime.UtcNow,
			UpdatedBy = null,
			AdvanceRequieredNigth = 0,
			ManagerPartPerClient= 0,
			MaxRoomClients= 0,
			NigthCostFor3Client= 0,
			NigthCostMost4Client= 0,
			NigthCostUnder2Client= 0,
			NigthDiscountFor3Client= 0,
			NigthDiscountMost4Client= 0,
			NigthDiscountUnder2Client= 0,
			TransportCost = 0
		};
	


		public ReservationConfiguration GetOne()
		{
			return item;
		}


		public ReservationConfiguration Edit(ReservationConfigurationInput res)
		{
			item.AdvanceRequieredNigth = 10;
			return item;
		}


	}
}
