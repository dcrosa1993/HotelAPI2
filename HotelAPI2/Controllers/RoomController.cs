using HotelAPI2.Domain;
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
		public IEnumerable<Room> Get()
		{
			return this._roomRepository.GetAll();
		}

		// GET api/<RoomController>/5
		[HttpGet("{id}")]
		public Room Get(int id)
		{
			return this._roomRepository.GetOne(id);
		}

		// POST api/<RoomController>
		[HttpPost]
		public Room Post([FromBody] Room value)
		{
			return this._roomRepository.AddRoom(value);
		}

		// PUT api/<RoomController>/5
		[HttpPut("{id}")]
		public Room Put(int id, [FromBody] Room value)
		{
			return this._roomRepository.Edit(value, id);
		}

		// DELETE api/<RoomController>/5
		[HttpDelete("{id}")]
		public Room Delete(int id)
		{
			return this._roomRepository.Remove(id);
		}
	}
}
