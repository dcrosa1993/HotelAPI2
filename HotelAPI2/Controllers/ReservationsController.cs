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
	public class ReservationsController : ControllerBase
	{
		private ReservationRepository _reservationRepository;
		private ClientRepository _clientRepository;
		private ReservationConfiguration _reservationConfiguration;
		private UserRepository _userRepository;
		public ReservationsController(ReservationRepository rep, ClientRepository repClient, ReservationConfigurationRepository repConf, HotelContext hc, UserRepository userRepository)
		{
			this._reservationRepository = rep;
			this._clientRepository = repClient;
			this._reservationConfiguration=repConf.GetOne(hc).Success;
			this._userRepository = userRepository;
		}
		// GET: api/<ReservationsController>
		[HttpGet]
		public Response<List<ReservationsOutput>> Get(HotelContext hc, Mappers mappers)
		{
			return this._reservationRepository.GetAll(hc, mappers);
		}

		// GET api/<ReservationsController>/5
		[HttpGet("{id}")]
		public Response<ReservationsOutput> Get(int id, HotelContext hc, Mappers mappers)
		{
			return this._reservationRepository.GetOne(id, hc, mappers);
		}

		// POST api/<ReservationsController>
		[HttpPost]
		public Response<ReservationsOutput> Post([FromBody] ReservationsInput res, HotelContext hc, Mappers mappers)
		{
			//require validate if is possible added the reservations in the selected period
			//reqeuire validate the client count in reservation (include empty)

			int discountPercent = this.getDiscount(res.NoClients);
			double management = this._reservationConfiguration.ManagerPartPerClient;
			double transport = this._reservationConfiguration.TransportCost;
			TimeSpan intervalo = res.DateOut.Date - res.DateIn.Date;
			int totalNights = intervalo.Days;
			double costPerClient = this.getCost(res.NoClients);

			Reservation reservation = new Reservation()
			{
				AdvanceManagement = res.AdvanceManagement,
				DateIn = res.DateIn,
				DateOut = res.DateOut,
				Description = res.Description,
				Discount = discountPercent,
				Management = this._reservationConfiguration.ManagerPartPerClient,
				NoClients = res.NoClients,
				CostPerClient = costPerClient,
				Transport = transport,
				TotalNights = totalNights,
				PaymentNights = res.PaymentNights,
				CreatedBy = GetLoggedInUser().Email,
				UpdatedBy = GetLoggedInUser().Email,
				UpdatedTime = DateTime.UtcNow,
				CreatedTime = DateTime.UtcNow,
				User = this._userRepository.GetUserByEmail(GetLoggedInUser().Email, hc, mappers).Success
			};
			var response = this._reservationRepository.AddReservation(reservation, hc, mappers);
			if (!response.Error)
			{
				foreach(ClientInput client in res.Clients)
				{
					var existClient = this._clientRepository.GetOneByPassport(client.Passport, hc);
					if(existClient.Success is Client)
					{
						existClient.Success.Reservations.Add(reservation);
						var responseEdit = this._clientRepository.Edit(existClient.Success, existClient.Success.Id, GetLoggedInUser().Email, hc);
						if (responseEdit.Error)
						{
							this._reservationRepository.Remove(reservation.Id, hc);
							response.Message = "Ha ocurrido un error agregando a los clientes, intentelo mas tarde";
							response.Error = true;
							response.Success = null;
							return response;
						}
						else
						{
							response.Success.Clients.Add(mapClient(existClient.Success));
						}
						
					}
					else
					{
						Client newClient = new Client()
						{
							Age = client.Age,
							Email = client.Email,
							Name = client.Name,
							Passport = client.Passport,
							Phone = client.Phone,
							CreatedBy = GetLoggedInUser().Email,
							UpdatedBy = GetLoggedInUser().Email,
							UpdatedTime = DateTime.UtcNow,
							CreatedTime = DateTime.UtcNow,
							Reservations = new List<Reservation>() { reservation }
						};
						var responseAdd = this._clientRepository.AddClient(newClient, hc);
						if (responseAdd.Error)
						{
							this._reservationRepository.Remove(reservation.Id, hc);
							response.Message = "Ha ocurrido un error agregando a los clientes, intentelo mas tarde";
							response.Error = true;
							response.Success = null;
							return response;
						}
						else
						{
							response.Success.Clients.Add(mapClient(responseAdd.Success));
						}
					}

					
				}
			}
			else
			{
				response.Message = "No se ha podido agregar la reservacion";
				return response;
			}
			return response;
		}
		/*
		// PUT api/<ReservationsController>/5
		[HttpPut("{id}")]
		public Response<ReservationsOutput> Put(int id, [FromBody] Reservation value, HotelContext hc)
		{
			return this._reservationRepository.Edit(value, id, hc);
		}
		*/
		// DELETE api/<ReservationsController>/5
		[HttpDelete("{id}")]
		public Response<bool> Delete(int id, HotelContext hc)
		{
			return this._reservationRepository.Remove(id, hc);
		}

		private int getDiscount(int clientCount) { 
			if(clientCount <= 2) { 
				return this._reservationConfiguration.NigthDiscountUnder2Client;
			}else if(clientCount == 3)
			{
				return this._reservationConfiguration.NigthDiscountFor3Client;
			}else
			{
				return this._reservationConfiguration.NigthDiscountMost4Client;
			}
		}

		private int getCost(int clientCount)
		{
			if (clientCount <= 2)
			{
				return this._reservationConfiguration.NigthCostUnder2Client;
			}
			else if (clientCount == 3)
			{
				return this._reservationConfiguration.NigthCostFor3Client;
			}
			else
			{
				return this._reservationConfiguration.NigthCostMost4Client;
			}
		}

		private ClientOutput mapClient(Client client)
		{
			return new ClientOutput()
			{
				Age = client.Age,
				Email = client.Email,
				Id = client.Id,
				Name = client.Name,
				Passport = client.Passport,
				Phone = client.Phone,
			};

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
