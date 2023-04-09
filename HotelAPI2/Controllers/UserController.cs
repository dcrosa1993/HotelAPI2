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
	public class UserController : ControllerBase
	{
		private UserRepository _userRepository;
		public UserController(UserRepository rep) {
			this._userRepository = rep;
		}
		// GET: api/<UserController>
		[HttpGet]
		public Response<List<User>> Get(HotelContext hc)
		{
			return this._userRepository.GetAll(hc);
		}

		// GET api/<UserController>/5
		[HttpGet("{id}")]
		public Response<User> Get(int id, HotelContext hc)
		{
			return this._userRepository.GetOne(id, hc);
		}

		// POST api/<UserController>
		[HttpPost]
		public Response<User> Post([FromBody] UserInput value, HotelContext hc)
		{
			return this._userRepository.AddUser(value, hc);
		}

		// PUT api/<UserController>/5
		[HttpPut("{id}")]
		public Response<User> Put(int id, [FromBody] UserInput value, HotelContext hc)
		{
			return this._userRepository.Edit(value, id, hc);
		}

		// DELETE api/<UserController>/5
		[HttpDelete("{id}")]
		public Response<bool> Delete(int id, HotelContext hc)
		{
			return this._userRepository.Remove(id, hc);
		}
	}
}
