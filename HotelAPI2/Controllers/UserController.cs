using HotelAPI2.Domain;
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
		public IEnumerable<User> Get()
		{
			return this._userRepository.GetAll();
		}

		// GET api/<UserController>/5
		[HttpGet("{id}")]
		public User Get(int id)
		{
			return this._userRepository.GetOne(id);
		}

		// POST api/<UserController>
		[HttpPost]
		public User Post([FromBody] User value)
		{
			return this._userRepository.AddUser(value);
		}

		// PUT api/<UserController>/5
		[HttpPut("{id}")]
		public User Put(int id, [FromBody] User value)
		{
			return this._userRepository.Edit(value, id);
		}

		// DELETE api/<UserController>/5
		[HttpDelete("{id}")]
		public User Delete(int id)
		{
			return this._userRepository.Remove(id);
		}
	}
}
