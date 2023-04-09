using HotelAPI2.Domain;

namespace HotelAPI2.Repositories
{
	public class RoomRepository
	{
		private Room item = new Room()
		{
			Id = 1,
			CreatedBy = 0,
			CreatedTime = DateTime.UtcNow,
			UpdatedTime = DateTime.UtcNow,
			UpdatedBy = 0,
			Availability = true,
			Capacity= 10,
			Number="111",
			Reservation=new Reservation()
		};
		public Room AddRoom(Room res)
		{
			return item;
		}


		public Room GetOne(int id)
		{
			return item;
		}

		public Room Remove(int id)
		{
			return item;
		}

		public Room Edit(Room res, int id)
		{
			return item;
		}

		public Room[] GetAll()
		{
			return new Room[] { item, item, item };
		}
	}
}
