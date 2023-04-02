using HotelAPI2.Domain;

namespace HotelAPI2.Repositories
{
	public class ClientRepository
	{
		private Client item = new Client()
		{
			Id = 1,
			CreatedBy = null,
			CreatedTime = DateTime.UtcNow,
			UpdatedTime = DateTime.UtcNow,
			UpdatedBy = null,
			Age="Adult",
			Email="example@gmail.com",
			Name="Fulano de tal",
			Passport="20000DDE",
			Phone="020201154",
			Reservations=new Reservation[] {}			
		};
		public Client AddClient(Client res)
		{
			return item;
		}


		public Client GetOne(int id)
		{
			return item;
		}

		public Client Remove(int id)
		{
			return item;
		}

		public Client Edit(Client res, int id)
		{
			return item;
		}

		public Client[] GetAll()
		{
			return new Client[] { item, item, item };
		}
	}
}
