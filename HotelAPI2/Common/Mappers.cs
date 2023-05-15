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
				Management = reservation.Management,
				NoClients = reservation.Clients!.Count(),
				PaymentNights = reservation.PaymentNights,
				RoomId = reservation.RoomId,
				TotalNights = reservation.TotalNights,
				Transport = reservation.Transport,
				UserEmail = reservation.User.Email,
			};

		}
	}
}
