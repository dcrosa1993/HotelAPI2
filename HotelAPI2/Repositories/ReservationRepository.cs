using HotelAPI2.Domain;

namespace HotelAPI2.Repositories
{
	public class ReservationRepository
	{
		private Reservation item = new Reservation()
		{
			AdvanceManagement = false,
			DateIn = DateTime.UtcNow,
			DateOut = DateTime.UtcNow,
			Description = "Example",
			Discount = 0,
			Management = 0,
			Id = 1,
			CreatedBy = 0,
			CreatedTime = DateTime.UtcNow,
			TotalCost = 0,
			Transport = 0,
			TotalNights = 0,
			NoClients = 0,
			PaymentNights = 0,
			UpdatedTime = DateTime.UtcNow,
			UpdatedBy = 0,
			Clients = Array.Empty<Client>(),
			Room = new Room()
		};
		public Reservation AddReservation(Reservation res)
		{
			return item;
		}


		public Reservation GetOne(int id){
			return item;
		}

		public Reservation Remove(int id)
		{
			return item;
		}

		public Reservation Edit(Reservation res, int id)
		{
			return item;
		}

		public Reservation[] GetAll()
		{
			return new Reservation[] {item,item,item};
		}

	}

	
}
