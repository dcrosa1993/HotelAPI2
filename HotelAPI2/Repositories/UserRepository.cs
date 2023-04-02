using HotelAPI2.Domain;

namespace HotelAPI2.Repositories
{
	public class UserRepository
	{
		private User item = new User()
		{
			Id = 1,
			CreatedBy = null,
			CreatedTime = DateTime.UtcNow,
			UpdatedTime = DateTime.UtcNow,
			UpdatedBy = null,
			Email = "example@gmail.com",
			Name = "Fulano de tal",
			Phone = "020201154",
			Access = {},
			Baned= false,
			ChangePassword = false,
			Password = "asd123",
			Role = {},
			Reservations = new Reservation[] { }
		};
		public User AddUser(User res)
		{
			return item;
		}


		public User GetOne(int id)
		{
			return item;
		}

		public User Remove(int id)
		{
			return item;
		}

		public User Edit(User res, int id)
		{
			return item;
		}

		public User[] GetAll()
		{
			return new User[] { item, item, item };
		}
	}
}
