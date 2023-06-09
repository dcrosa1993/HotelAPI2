﻿using HotelAPI2.Common;
using HotelAPI2.Domain;
using HotelAPI2.DTOs;
using HotelAPI2.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HotelAPI2.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ReservationConfigurationController : ControllerBase
	{
		private ReservationConfigurationRepository _reservationConfigurationRepository;
		public ReservationConfigurationController(ReservationConfigurationRepository rep)
		{
			this._reservationConfigurationRepository = rep;
		}

		// GET api/<ReservationConfigurationController>/5
		[HttpGet()]
		public Response<ReservationConfiguration> Get(HotelContext hc)
		{
			return _reservationConfigurationRepository.GetOne(hc);
		}

		// PUT api/<ReservationConfigurationController>/5
		[HttpPut()]
		public Response<ReservationConfiguration> Put([FromBody] ReservationConfigurationInput value, HotelContext hc)
		{
			return _reservationConfigurationRepository.Edit(value, GetLoggedInUser().Email, hc);
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
