using HotelAPI2.Common;
using HotelAPI2.Domain;
using HotelAPI2.DTOs;
using Microsoft.EntityFrameworkCore;

namespace HotelAPI2.Repositories
{
	public class RoomRepository
	{
		public Response<Room> AddRoom(Room res, HotelContext hc)
		{
			Response<Room> result = new Response<Room>();
			
			hc.Rooms.Add(res);
			if (hc.SaveChangesAsync().Result != 0)
			{
				
				result.Success = res;
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

		public Response<Room> Edit(RoomInput res, int id, string email, HotelContext hc)
		{
			Response<Room> result = new Response<Room>();
			var room = hc.Rooms.FindAsync(id).Result;
			if (room is Room)
			{
				room.Number = res.Number;
				room.Availability = res.Availability;
				room.UpdatedTime = DateTime.UtcNow;
				room.Capacity = res.Capacity;
				
				room.UpdatedBy = email;

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
