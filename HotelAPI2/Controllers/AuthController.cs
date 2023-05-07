using HotelAPI2.Common;
using HotelAPI2.Domain;
using HotelAPI2.DTOs;
using HotelAPI2.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HotelAPI2.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private UserRepository _userRepository;
		private IConfiguration _configuration;
		public AuthController(UserRepository rep, IConfiguration configuration) {
			this._userRepository = rep;
			this._configuration = configuration;
		}
		

		[HttpPost]
		public IResult Post([FromBody] Credentials value, HotelContext hc, Mappers mappers)
		{
			return this._userRepository.Login(value, hc, _configuration, mappers);
		}

		[HttpGet]
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
		public LoggedInUser Get() => GetLoggedInUser();

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
			return new LoggedInUser() { Email=email, Name=userId, Role=userType};
		}


	}
}
