using HotelAPI2.Domain;
using HotelAPI2.Repositories;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HotelAPI2.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ReservationsController : ControllerBase
	{
		private ReservationRepository _reservationRepository;
		public ReservationsController(ReservationRepository rep)
		{
			this._reservationRepository = rep;
		}
		// GET: api/<ReservationsController>
		[HttpGet]
		public IEnumerable<Reservation> Get()
		{
			return this._reservationRepository.GetAll();
		}

		// GET api/<ReservationsController>/5
		[HttpGet("{id}")]
		public Reservation Get(int id)
		{
			return this._reservationRepository.GetOne(id);
		}

		// POST api/<ReservationsController>
		[HttpPost]
		public Reservation Post([FromBody] Reservation value)
		{
			return this._reservationRepository.AddReservation(value);
		}

		// PUT api/<ReservationsController>/5
		[HttpPut("{id}")]
		public Reservation Put(int id, [FromBody] Reservation value)
		{
			return this._reservationRepository.Edit(value, id);
		}

		// DELETE api/<ReservationsController>/5
		[HttpDelete("{id}")]
		public Reservation Delete(int id)
		{
			return this._reservationRepository.Remove(id);
		}
	}
}
