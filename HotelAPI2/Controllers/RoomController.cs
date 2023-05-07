using HotelAPI2.Common;
using HotelAPI2.Domain;
using HotelAPI2.DTOs;
using HotelAPI2.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HotelAPI2.Controllers
{
	[Route("api/[controller]")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
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
			Room room = new Room()
			{
				Number = value.Number,
				Availability = value.Availability,
				Capacity = value.Capacity,
				CreatedBy = GetLoggedInUser().Email,
				UpdatedBy = GetLoggedInUser().Email,
				UpdatedTime = DateTime.UtcNow,
				CreatedTime = DateTime.UtcNow,
			};
			return this._roomRepository.AddRoom(room, hc);
		}

		// PUT api/<RoomController>/5
		[HttpPut("{id}")]
		public Response<Room> Put(int id, [FromBody] RoomInput value, HotelContext hc)
		{
			return this._roomRepository.Edit(value, id, GetLoggedInUser().Email, hc);
		}

		// DELETE api/<RoomController>/5
		[HttpDelete("{id}")]
		public Response<bool> Delete(int id, HotelContext hc)
		{
			return this._roomRepository.Remove(id, hc);
		}

		protected LoggedInUser GetLoggedInUser()
		{
			string userId = string.Empty;
			string userType = string.Empty;
			string email = string.Empty;

			var identity = HttpContext.User.Identity as ClaimsIdentity;
			if (identity != null)
			{
				IEnumerable<Claim> claims = identity.Claims;
				userId = claims?.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
				//contactId = claims?.FirstOrDefault(x => x.Type.Equals("ContactId", StringComparison.OrdinalIgnoreCase))?.Value;
				userType = claims?.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;
				email = claims?.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
			}
			return new LoggedInUser() { Email = email, Name = userId, Role = userType };
		}
	}
}
