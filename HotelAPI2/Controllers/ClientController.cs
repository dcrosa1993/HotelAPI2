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
	public class ClientController : ControllerBase
	{
		private ClientRepository _clientRepository;
		public ClientController(ClientRepository rep)
		{
			this._clientRepository = rep;
		}
		// GET: api/<ClientController>
		[HttpGet]
		public Response<List<Client>> Get(HotelContext hc)
		{
			return this._clientRepository.GetAll(hc);
		}

		// GET api/<ClientController>/5
		[HttpGet("{id}")]
		public Response<Client> Get(int id, HotelContext hc)
		{
			return this._clientRepository.GetOne(id, hc);
		}

		/*
		// POST api/<ClientController>
		[HttpPost]
		public Response<Client> Post([FromBody] ClientInput value, HotelContext hc)
		{
			return this._clientRepository.AddClient(value, hc);
		}
		*/

		/*
		// PUT api/<ClientController>/5
		[HttpPut("{id}")]
		public Response<Client> Put(int id, [FromBody] ClientInput value, HotelContext hc)
		{
			return this._clientRepository.Edit(value, id, hc);
		}
		*/

		// DELETE api/<ClientController>/5
		[HttpDelete("{id}")]
		public Response<bool> Delete(int id, HotelContext hc)
		{
			return this._clientRepository.Remove(id, hc);
		}
	}
}
