using DinkToPdf.Contracts;
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
	//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
	[ApiController]
	public class ReservationsController : ControllerBase
	{
		private ReservationRepository _reservationRepository;
		private ClientRepository _clientRepository;
		private ReservationConfiguration _reservationConfiguration;
		private UserRepository _userRepository;
		private RoomRepository _roomRepository;
		private readonly IConverter _converter;
		public ReservationsController(RoomRepository roomRepository, IConverter converter, ReservationRepository rep, ClientRepository repClient, ReservationConfigurationRepository repConf, HotelContext hc, UserRepository userRepository)
		{
			this._reservationRepository = rep;
			this._clientRepository = repClient;
			this._reservationConfiguration=repConf.GetOne(hc).Success;
			this._userRepository = userRepository;
			this._roomRepository = roomRepository;
			_converter = converter;
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

		[HttpGet("GeneratePDF/{id}")]
		public IActionResult GeneratePDF(int id, HotelContext hc, Mappers mappers)
		{
			try
			{
				// Verifica si la información de la reserva es válida

				var reservation = this._reservationRepository.GetOne(id, hc, mappers).Success;

				if (reservation == null)
				{
					return BadRequest("Se requiere la información de la reserva.");
				}

				// Genera el archivo PDF

				var htmlToPdf = new HtmlToPdf();
				var outputPath = "archivo.pdf";
				var pdfDocument = htmlToPdf.RenderHtmlAsPdf(this.GenerateHtmlContent(reservation)); // Reemplaza "HTML aquí" con el contenido HTML que deseas convertir a PDF

				pdfDocument.SaveAs(outputPath);

				// Devuelve el archivo PDF generado

				var fileBytes = System.IO.File.ReadAllBytes(outputPath);
				return File(fileBytes, "application/pdf", "reservation.pdf");
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error al generar el PDF: {ex.Message}");
			}
		}

		[HttpGet("GetDatesWithOutDisponibility")]
		public Response<List<DateTime>> GetDatesWithOutDisponibility(HotelContext hc, Mappers mappers)
		{
			return this.DatesWithOutDisponibility(hc, mappers);
		}

		private Response<List<DateTime>> DatesWithOutDisponibility(HotelContext hc, Mappers mappers)
		{
			Response<List<DateTime>> result = new Response<List<DateTime>>();

			var reservaciones = this._reservationRepository.GetCurrentReservations(hc, mappers).Success;
			if (reservaciones == null)
			{
				result.Success = new List<DateTime>();
				return result;
			}
			var cantidadHabitaciones = this._roomRepository.GetAll(hc).Success?.Count();
			if (cantidadHabitaciones == null)
			{
				result.Error = true;
				result.Message = "No se han encontrado habitaciones registradas";
				return result;
			}
			// Obtener todas las fechas ocupadas en un solo arreglo
			var fechasOcupadas = reservaciones.SelectMany(reservacion => GetDatesInRange(reservacion.DateIn, reservacion.DateOut));

			// Contar las fechas ocupadas y encontrar las fechas sin disponibilidad
			result.Success = fechasOcupadas
				.GroupBy(fecha => fecha)
				.Where(grupo => grupo.Count() >= cantidadHabitaciones)
				.Select(grupo => grupo.Key)
				.ToList();

			return result;
		}

		// Función auxiliar para obtener todas las fechas entre una fecha de entrada y una fecha de salida
		private IEnumerable<DateTime> GetDatesInRange(DateTime startDate, DateTime endDate)
		{
			var dates = new List<DateTime>();
			var currentDate = startDate;

			while (currentDate <= endDate)
			{
				dates.Add(currentDate);
				currentDate = currentDate.AddDays(1);
			}

			return dates;
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

		private string GenerateHtmlContent(ReservationsOutput reservation)
		{
			// Genera el contenido HTML del PDF utilizando una plantilla o código HTML
			// Puedes usar una librería de plantillas como RazorEngine o generar el HTML manualmente

			return $@"
            <html>
            <body>
                <h1>Información de la reserva</h1>
                <p>ID de reserva: {reservation.Id}</p>
                <p>Nombre del cliente: {reservation.UserEmail}</p>
                <!-- Agrega más información de la reserva según tus necesidades -->
            </body>
            </html>";
		}
	}
}
