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
	public class UserController : ControllerBase
	{
		private UserRepository _userRepository;
		private IConfiguration _configuration;
		public UserController(UserRepository rep, IConfiguration configuration) {
			this._userRepository = rep;
			this._configuration = configuration;
		}
		// GET: api/<UserController>
		[HttpGet]
		public Response<List<UserOutput>> Get(HotelContext hc, Mappers mappers)
		{
			return this._userRepository.GetAll(hc, mappers);
		}

		// GET api/<UserController>/5
		[HttpGet("{id}")]
		public Response<UserOutput> Get(int id, HotelContext hc, Mappers mappers)
		{
			return this._userRepository.GetOne(id, hc, mappers);
		}

		// POST api/<UserController>
		[HttpPost]
		public Response<UserOutput> Post([FromBody] UserInput value, HotelContext hc, Mappers mappers)
		{
			return this._userRepository.AddUser(value, GetLoggedInUser().Email, hc, mappers);
		}

		// PUT api/<UserController>/5
		[HttpPut("{id}")]
		public Response<UserOutput> Put(int id, [FromBody] UserInput value, HotelContext hc, Mappers mappers)
		{
			return this._userRepository.Edit(value, id, GetLoggedInUser().Email, hc, mappers);
		}

		// DELETE api/<UserController>/5
		[HttpDelete("{id}")]
		public Response<bool> Delete(int id, HotelContext hc)
		{
			return this._userRepository.Remove(id, hc);
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
