using HotelAPI2.Common;
using HotelAPI2.Domain;
using HotelAPI2.DTOs;
using Microsoft.EntityFrameworkCore;

namespace HotelAPI2.Repositories
{
	public class ReservationConfigurationRepository
	{
		public Response<ReservationConfiguration> GetOne(HotelContext hc)
		{
			Response<ReservationConfiguration> result = new Response<ReservationConfiguration>();
			var conf = hc.ReservationConfiguration.ToListAsync().Result.FirstOrDefault();
			if (conf is ReservationConfiguration)
			{
				result.Success = conf;
			}
			else
			{
				result.Error = true;
				result.Message = "Error in operation";
			}
			return result;
		}


		public Response<ReservationConfiguration> Edit(ReservationConfigurationInput res, string email, HotelContext hc)
		{
			Response<ReservationConfiguration> result = new Response<ReservationConfiguration>();
			var conf = hc.ReservationConfiguration.ToListAsync().Result.FirstOrDefault();
			if (conf is ReservationConfiguration)
			{
				conf.TransportCost = res.TransportCost;
				conf.NigthCostFor3Client = res.NigthCostFor3Client;
				conf.NigthDiscountFor3Client = res.NigthDiscountFor3Client;
				conf.NigthDiscountMost4Client = res.NigthDiscountMost4Client;
				conf.NigthCostMost4Client = res.NigthCostMost4Client;
				conf.NigthCostUnder2Client = res.NigthCostUnder2Client;
				conf.NigthDiscountUnder2Client= res.NigthDiscountUnder2Client;
				conf.AdvanceRequieredNigth = res.AdvanceRequieredNigth;
				conf.MaxRoomClients = res.MaxRoomClients;
				conf.ManagerPartPerClient = res.ManagerPartPerClient;
				conf.UpdatedTime= DateTime.UtcNow;
				conf.UpdatedBy = email;

				if (hc.SaveChangesAsync().Result != 0)
				{
					result.Success = conf;
				}
				else
				{
					result.Error = true;
					result.Message = "Error in operation";
				}

			}
			else
			{
				result.Error = true;
				result.Message = "Error in operation";
			}

			return result;
		}

		public int GetDiscount(int countClient) {
			return 10;
		}

	}
}
