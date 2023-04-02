using HotelAPI2.Domain;
using HotelAPI2.DTOs;
using HotelAPI2.Repositories;
using Microsoft.AspNetCore.Mvc;

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
		public ReservationConfiguration Get()
		{
			return _reservationConfigurationRepository.GetOne();
		}

		// PUT api/<ReservationConfigurationController>/5
		[HttpPut()]
		public ReservationConfiguration Put([FromBody] ReservationConfigurationInput value)
		{
			return _reservationConfigurationRepository.Edit(value);
		}

		
	}
}
