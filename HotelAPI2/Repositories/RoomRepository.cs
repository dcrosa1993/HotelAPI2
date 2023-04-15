using HotelAPI2.Common;
using HotelAPI2.Domain;
using HotelAPI2.DTOs;
using Microsoft.EntityFrameworkCore;

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
		public Response<Room> AddRoom(RoomInput res, HotelContext hc)
		{
			Response<Room> result = new Response<Room>();
			Room room = new Room()
			{
				Number = res.Number,
				Availability= res.Availability,
				Capacity= res.Capacity,				
				CreatedBy = 0,
				UpdatedBy = 0,
				UpdatedTime = DateTime.UtcNow,
				CreatedTime = DateTime.UtcNow,

			};
			hc.Rooms.Add(room);
			if (hc.SaveChangesAsync().Result != 0)
			{
				result.Success = room;
			}
			else
			{
				result.Error = true;
				result.Message = "Error in operation";
			}

			return result;
		}


		public Response<Room> GetOne(int id, HotelContext hc)
		{
			Response<Room> result = new Response<Room>();
			var room = hc.Rooms.FindAsync(id).Result;
			if (room is Room)
			{
				result.Success = room;
			}
			else
			{
				result.Error = true;
				result.Message = "Error in operation";
			}
			return result;
		}

		public Response<bool> Remove(int id, HotelContext hc)
		{
			Response<bool> result = new Response<bool>();
			var room = hc.Rooms.FindAsync(id).Result;
			if (room is Room)
			{
				hc.Rooms.Remove(room);

				if (hc.SaveChangesAsync().Result != 0)
				{
					result.Success = true;
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

		public Response<Room> Edit(RoomInput res, int id, HotelContext hc)
		{
			Response<Room> result = new Response<Room>();
			var room = hc.Rooms.FindAsync(id).Result;
			if (room is Room)
			{
				room.Number = res.Number;
				room.Availability = res.Availability;
				room.UpdatedTime = DateTime.UtcNow;
				room.Capacity = res.Capacity;
				
				room.UpdatedBy = 0;

				if (hc.SaveChangesAsync().Result != 0)
				{
					result.Success = room;
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

		public Response<List<Room>> GetAll(HotelContext hc)
		{
			Response<List<Room>> result = new Response<List<Room>>();
			var rooms = hc.Rooms.ToListAsync().Result;
			if (rooms is List<Room>)
			{
				result.Success = rooms;
			}
			else
			{
				result.Error = true;
				result.Message = "Error in operation";
			}
			return result;
		}
	}
}
