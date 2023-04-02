using HotelAPI2.Domain;
using HotelAPI2.Repositories;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HotelAPI2.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ClientController : ControllerBase
	{
		private ClientRepository _clientRepository;
		public ClientController(ClientRepository rep)
		{
			this._clientRepository = rep;
		}
		// GET: api/<ClientController>
		[HttpGet]
		public IEnumerable<Client> Get()
		{
			return this._clientRepository.GetAll();
		}

		// GET api/<ClientController>/5
		[HttpGet("{id}")]
		public Client Get(int id)
		{
			return this._clientRepository.GetOne(id);
		}

		// POST api/<ClientController>
		[HttpPost]
		public Client Post([FromBody] Client value)
		{
			return this._clientRepository.AddClient(value);
		}

		// PUT api/<ClientController>/5
		[HttpPut("{id}")]
		public Client Put(int id, [FromBody] Client value)
		{
			return this._clientRepository.Edit(value, id);
		}

		// DELETE api/<ClientController>/5
		[HttpDelete("{id}")]
		public Client Delete(int id)
		{
			return this._clientRepository.Remove(id);
		}
	}
}
