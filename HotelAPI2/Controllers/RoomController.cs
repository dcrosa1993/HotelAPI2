using HotelAPI2.Common;
using HotelAPI2.Domain;
using HotelAPI2.DTOs;
using HotelAPI2.Repositories;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HotelAPI2.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class RoomController : ControllerBase
	{
		private RoomRepository _roomRepository;
		public RoomController(RoomRepository rep)
		{
			this._roomRepository = rep;
		} 
		// GET: api/<RoomController>
		[HttpGet]
		public Response<List<Room>> Get(HotelContext hc)
		{
			return this._roomRepository.GetAll(hc);
		}

		// GET api/<RoomController>/5
		[HttpGet("{id}")]
		public Response<Room> Get(int id, HotelContext hc)
		{
			return this._roomRepository.GetOne(id, hc);
		}

		// POST api/<RoomController>
		[HttpPost]
		public Response<Room> Post([FromBody] RoomInput value, HotelContext hc)
		{
			return this._roomRepository.AddRoom(value, hc);
		}

		// PUT api/<RoomController>/5
		[HttpPut("{id}")]
		public Response<Room> Put(int id, [FromBody] RoomInput value, HotelContext hc)
		{
			return this._roomRepository.Edit(value, id, hc);
		}

		// DELETE api/<RoomController>/5
		[HttpDelete("{id}")]
		public Response<bool> Delete(int id, HotelContext hc)
		{
			return this._roomRepository.Remove(id, hc);
		}
	}
}
