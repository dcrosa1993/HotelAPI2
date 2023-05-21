using HotelAPI2.Domain;
using HotelAPI2.DTOs;

namespace HotelAPI2.Common
{
	public class Mappers
	{
		public UserOutput mapUser(User user)
		{
			var reservationsOut = new List<ReservationsOutput>();
			if (user.Reservations != null)
			{
				foreach (var reservation in user.Reservations)
				{
					reservationsOut.Add(mapReservation(reservation));
				}
			}
			return new UserOutput()
			{
				Email = user.Email,
				Baned = user.Baned,
				ChangePassword = user.ChangePassword,
				Id = user.Id,
				Name = user.Name,
				Phone = user.Phone,
				Role = user.Role,
				UserAccess = user.UserAccess,
				Reservations = reservationsOut
			};

		}

		public UserOutput mapUserWithoutReservations(User user)
		{			
			return new UserOutput()
			{
				Email = user.Email,
				Baned = user.Baned,
				ChangePassword = user.ChangePassword,
				Id = user.Id,
				Name = user.Name,
				Phone = user.Phone,
				Role = user.Role,
				UserAccess = user.UserAccess,				
			};

		}

		public ClientOutput mapClient(Client client)
		{
			return new ClientOutput()
			{
				Age = client.Age,
				Email = client.Email,
				Id = client.Id,
				Name = client.Name,
				Passport = client.Passport,
				Phone = client.Phone,
				UpdatedTime= client.UpdatedTime,
				UpdatedBy= client.UpdatedBy,
				CreatedTime= client.CreatedTime,
				CreatedBy= client.CreatedBy,
			};

		}

		public ReservationsOutput mapReservation(Reservation reservation)
		{
			var clientsOut = new List<ClientOutput>();
			if (reservation.Clients != null)
			{
				foreach (var client in reservation.Clients)
				{
					clientsOut.Add(mapClient(client));
				}
			}
			var _lodgingCostPerClient = reservation.CostPerClient * reservation.TotalNights - (reservation.CostPerClient * (reservation.Discount / 100));
			var _totalCostPerClient = _lodgingCostPerClient + reservation.Transport;
			var _totalPaydedPerClient = reservation.CostPerClient * reservation.PaymentNights - (reservation.CostPerClient * (reservation.Discount / 100)) + reservation.Transport;
			var _pendingPerClient = _totalCostPerClient - _totalPaydedPerClient;
			var _totalCost = _totalCostPerClient * reservation.Clients!.Count();

			var _totalPending = _pendingPerClient * reservation.Clients!.Count();
			var _totalPayded = _totalPaydedPerClient * reservation.Clients!.Count();

			var _management = reservation.Management * reservation.Clients!.Count();

			return new ReservationsOutput()
			{
				AdvanceManagement = reservation.AdvanceManagement,
				CostPerClient = reservation.CostPerClient,
				DateIn = reservation.DateIn,
				DateOut = reservation.DateOut,
				Clients = clientsOut,
				Description = reservation.Description,
				Discount = reservation.Discount,
				Id = reservation.Id,
				Management = _management,
				NoClients = reservation.Clients!.Count(),
				PaymentNights = reservation.PaymentNights,
				RoomId = reservation.RoomId,
				TotalNights = reservation.TotalNights,
				Transport = reservation.Transport,
				UserEmail = reservation.User.Email,
				CreatedBy= reservation.CreatedBy,
				CreatedTime= reservation.CreatedTime,
				UpdatedBy= reservation.UpdatedBy,
				UpdatedTime = reservation.UpdatedTime,
				LodgingCostPerClient = _lodgingCostPerClient,
				TotalCostPerClient = _totalCostPerClient,
				TotalPaydedPerClient = _totalPaydedPerClient,
				PendingPerClient = _pendingPerClient,
				TotalPayded = _totalPayded,
				TotalCost = _totalCost,
				TotalPending = _totalPending,

				/*
				TotalCost = reservation.Clients!.Count()* (reservation.CostPerClient - (reservation.CostPerClient * reservation.Discount / 100)) * reservation.TotalNights,
				TotalPayded = (reservation.Clients!.Count() * (reservation.CostPerClient - (reservation.CostPerClient * reservation.Discount / 100)) * reservation.PaymentNights)+ reservation.Transport
				TotalPayded = (reservation.Clients!.Count() * (reservation.CostPerClient - (reservation.CostPerClient * reservation.Discount / 100)) * reservation.PaymentNights) + reservation.Transport
				*/
			};

		}
	}
}
