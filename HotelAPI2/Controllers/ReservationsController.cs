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
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin,manager")]
		public Response<List<ReservationsOutput>> Get(HotelContext hc, Mappers mappers)
		{
			var user = this.GetLoggedInUser();
			return this._reservationRepository.GetAll(hc, mappers, user);
		}

		// GET api/<ReservationsController>/5
		[HttpGet("{id}")]
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin,manager")]
		public Response<ReservationsOutput> Get(int id, HotelContext hc, Mappers mappers)
		{
			return this._reservationRepository.GetOne(id, hc, mappers);
		}

		// POST api/<ReservationsController>
		[HttpPost]
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin,manager")]
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
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
		public Response<bool> Delete(int id, HotelContext hc)
		{
			return this._reservationRepository.Remove(id, hc);
		}

		[HttpGet("GeneratePDF/{id}")]
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin,manager")]
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
				htmlToPdf.RenderingOptions.MarginBottom = 0;
				htmlToPdf.RenderingOptions.MarginLeft = 0;
				htmlToPdf.RenderingOptions.MarginTop = 0;
				htmlToPdf.RenderingOptions.MarginRight = 0;
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
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin,manager")]
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
			if (cantidadHabitaciones == 0)
			{
				result.Error = true;
				result.Message = "No se han encontrado habitaciones registradas";
				return result;
			}
			// Obtener todas las fechas ocupadas en un solo arreglo
			var fechasOcupadas = reservaciones.SelectMany(reservacion => GetDatesInRange(reservacion.DateIn.Date, reservacion.DateOut.Date));

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

            var html = $@"
            
                <!DOCTYPE html>
                <html>
                  <head>
                    <title>
                      {{{{ controller_name }}}}
                    </title>
                    <style>
                      .logo {{
                        position: fixed;
                        left: 15;
                        top: 70;
                        width: 110px;
                        height: 110px;
                      }}
                      .verticalline {{
                        border-right-style: solid;
                      }}

                      td,
                      th {{

                        text-align: left;
                      }}

                      .centrar {{
                        text-align: center;
                      }}

                      .derecha {{
                        text-align: right;
                      }}

                      .letrica {{
                        font-size: 10px;
                      }}
                      .letrota{{
                        font-size: 14px;
                      }}

                      header {{
                        text-align: center;
                      }}

                      .tres {{
                        width: 33%;
                      }}

                      table {{
                        width: 100%;
                      }}

                      footer {{
                        position: absolute;
                        bottom: 0;
                        width: 100%;
                        height: 60px;
                        line-height: 60px;
                        background-color: #f5f5f5;
                      }}

                      body {{
                        font-size: 15px;
                      }}

                      .fondo-hotel {{
                        background-image: url('data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEASABIAAD//gBcYm9yZGVyIGJzOjAgYmM6IzAwMDAwMCBwczowIHBjOiNlZWVlZWUgZXM6MCBlYzojMDAwMDAwIGNrOjUwMGQwMmE0ZjFmMWQ3NDk3MzQwY2M1ODY4OTZiZjEx/+EBaEV4aWYAAE1NACoAAAAIAAwBMgACAAAAFAAAAJ4BBgADAAAAAQACAAABAAADAAAAAQV0AAABFQADAAAAAQADAAABAgADAAAAAwAAALIBKAADAAAAAQACAAABAQADAAAAAQMRAAABGwAFAAAAAQAAALgBEgADAAAAAQAAAAABMQACAAAAHAAAAMCHaQAEAAAAAQAAAOQBGgAFAAAAAQAAANwAAAEaMjAyMToxMToyNyAxNTo1MjowNAAACAAIAAgAAABIAAAAAUFkb2JlIFBob3Rvc2hvcCBDUzUgV2luZG93cwAAAABIAAAAAQAEkAAABwAAAAQwMjIxoAEAAwAAAAEAAQAAoAIABAAAAAEAAAV0oAMABAAAAAEAAAMRAAAAAAAEARsABQAAAAEAAAFQAQMAAwAAAAEABgAAASgAAwAAAAEAAgAAARoABQAAAAEAAAFYAAAAAAAAAEgAAAABAAAASAAAAAH/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAB0AZADASIAAhEBAxEB/8QAHAAAAAcBAQAAAAAAAAAAAAAAAAMEBQYHCAIB/8QAWRAAAQIEBAIFBgcJDAgFBQAAAQIDAAQFEQYSITETQQciUWFxFIGRobHRCBUjMkJSwRYkM5KTorLS4Rc0Q2Jyc4Kjs8LT8BgmU1Vjg5TDJTVEReNUVqTi8f/EABkBAAMBAQEAAAAAAAAAAAAAAAECAwAEBf/EAC4RAAIBAgUDBAEEAgMAAAAAAAABAgMREhMhMVEEFEEiMmGRBRUzcaFisSNSgf/aAAwDAQACEQMRAD8AWTPR1VmZd6Zbl5WcIccuZQll4BJy3CT1TonYHcwkw7iHEGFZsop8ytaRoafO3SR/yzYX70698aJk5cGUItu45+mYTVKhSdRl+DPyrMy2Bol1AVbwO48RHGk1qjqzb6SVyEUrpRpVUyS9dYepc1ffKVIJ8Nwe4ZvGJ/S5en1BrjSL7My1p123M2vYbbHuiC1nowkn2lfF77suLfgXhxmvAA9ZPiDEPdwhiLDUx5TT/KW8uzsgsuJA7Mmi0jttceMdEeqqQWFnLPoenqyxLRl5v01LrybIygb20vCr4uaUtC1jMpO3dFRUDpQq8mAiryzdRZToXmOq4nvUkD1W88T6ldIeGagyFipNy6+bUx1FD7PQYoupxLcl+nxpu7VyTLlWVkFbaSRqLiDAkDYWhk+67D1r/HEl+VEeHGGHv97yh8FwMafkqqVtUh4Ui5No9Si0MX3ZYcRp8bS/rP2QPu2w5/vVj0K90HGuRVRe9iQgR6YjRx3hkA3q7AA7Qr3QX+6BhUf+9yv53ugYlyUwPgkMtZLCrj+EcP5xj1rrkqsQL6REhj3C62crlal2znVp1jcZjY7cxrBzfSBhNCQPjyVP43ug4o23IuE5TWmhLCnSI50gyE9OYXmlUioPyE7LgzCFs3uvKDdBHO49doJPSHhS9vjqX/FV7oLX0kYRRcqrkvYb9VfuhXJNblVBp3SKGGLsQcsWLtp/DR4cWYhuf9biLdswR9sXcKcw9dxpptSF9dJCNwdRCdbMg0strclEuD6F05vRvHE2+TqzI8FMjF+Ib6YvP/U/tj37r8RDbFxPjMfti4wxLq0blnVns8mUPWoAeuB5CpfzKchHbx1IT6MuaNifJsyPBTf3X4k/+7U+eYv9scfdXiO4Jxd/+R/+0XN8VOq1Lck2ewNFz13T7I6TSE21WT3BCEj1Jv642J8gzI8FMDFmIjoMXj8v+2Czi+uDRWM7K2t5QLnw1i6jRZRR67CHB2OjOPzrwc1TWUtgNNNpRyCUACNifIc2PBRRxZX1A8PFb6z3kD1qsIAxNiPlihYB5LeF/VcRexpyD/Bo/FEASKb2yJ/FEbE+TZseCh11+urtxMVqPgm/vjlVXqpN14lfUfBXsFhF+eQj6o9EAydv/wCRsT5BmLgoNNRqGYqFaWFEWJSwRceYQDOzq9V1qYPhLrPsEX6JTxjoSZjXYc1cFAcZ9WiqrMqv2Sy/dHqUzCibVCdv2CXcv7I0B5HHok9PsgGzvgoFMtMn/wBZUyP5lz3QcJCcXqlytufyWHfdF9iT0joSIPbGNnfBQSqZUlDqjEPj5O57osvAS6dgDB81Xa65N+WTijw25jR1aRfKhKTtcgqJNtCCeUSasONUqnTE262t7hIKw0j5yyOQinJwVfGVZS++A88EANMXIalW+WbsHO25h4TUNTO9VcIa8TVqq40riJuopWtayUycg1qEJ7B3aC6jvbuAExoWAkGSQurzD631G5al3OG0gck6C58bxJ8M4TlqOgruXppwfKPrFiruA5DuiTsy2mnKBdvVglO2kdiISmDKJLm6KYytXa7d0/nEw9StNYlkZJZhplH1W0BI9Ah7TLCDEy47IxNyuNaJYdl4OSxbXLpDmmXFthBgl+6BcFxtSxBqGe6HBLFuUdpa7BGMIks35QYlqFiWjHQajGEYaPZACBmKbG4F4XBuPFNEOtntun1X+yCC4jyd0ecOFxa1gcKMa4fJt/eydPpKP5xg8I80CWTZhIg20USdhbhXDEcqYSeUGrIQLrISO0m0ECclSbJmWVHsSsE+gRrM1xmrmF6XVevOybbjvJ0XS4P6abGKpq3R4v4/dDNQVwLAoS+wHSPOCm/ni7FzjeoDcws9zC7eki0McyHHJ1a00+YBIFi4ttKfPZRPqiNTQtTm1sV610dTBQMs9T/6VMv/ANyDP3NXlbztMHhTP/kix2W506cGUR38ZS/VkHthQiWmzcOTLIH/AAmCD6VKI9UKkM6sisP3M3Lfv6Qv3Uz/AOSPR0YqI1n5Qn+LTkj++YtISJIsuZmHB3lKf0QI9NOYV+EbLg7HVFY9CrwbAzZclUO9HbUqRx6tJsnlnk20e1UcpwK2o9WrB1PaxSw6B50ggRbjUmyyLMNIbHYhIT7I7LN97nxjWNmS5KgHR+tQCm5h9aTsryKXSD5ioEeiOh0czR08sYQk8yy2SP6Ib1/Gi225RDaAhtAQgaBKRYDzR75P3RjZkuSpkdGSrEO1YKPa3T2k+3NBU10Zt8JZNUf0GgEq0n2CLeMvptCabYAZVpGNmS5EDFLlhLMpcZQ6UtpTmcGcmw77wqQyltAQ2kIQPop0Hohwba+TR4CPVNaQSRXVfxqqj45pWGhRxMOVJCnGZjyvIAEpUVZk8MkfNNrE3025HUbFipyZrKatSFUmUpQUp6admA4haU3upNki4sknWx20uYhXSlSZ6o9K+H5lElVU0yUYU0/NyaihSSoLsUKSoK3UAbd8NmDMN1OeYx/R5pqoyzNUKjIvzaioqTmXYqUSTmN0E31OvZAbHUVYmR6SZZFFTXXqFPt4fU4EeV50qdCCrIHC0Po3/jE90LsV46ksPNvTJpk1O09tpl3ytlaAhXFPVCbm50sb94iEuy1Vf6KZXBooc+muEolnSUgMIQlwK4gcvqCEjS19Tp2vnSfQ5iU6I5ahSMs9OziGZeWbCG8xs2UXUTsNEwL8mwkyw1W0Vt51pynTUi4nIUcUpUHEqSVBQI5aEeMOiX5ZSnUJLgW2SkgotmsSLpOx1BhoYn2aTh6kTq5WbcyoQgtNsniA5cpuOQB5+i8IaXiFyozbDRps8gKDrjr77KUpbT1lBAsblV1AXsbhPabwmJ4rGwkqUhsFsAK+UQHBdNrA8t94JQpTrobYQELSrKsrGYWVolQ/zuILNXksiC24tbjbQbCQy5qof0YJqyvJkomhOpp/Vyl5TqWwASNCVIWOXZ54yd0CwobTMcMZVIdIbSnKUg6qUdBbsA3gxpbLjzbYSsBSsgIsoE9vhEZpK2mHUNSOJKc6VJyLSy+yFOgXt1Q1fnsCPGJLNlmnNyrrhKG2lAnK2pZt4JBMZNhsdsFl7yrh5wZdwtrC02vra411EeOvsNSzTym31JcKhlSgEjLe5OvdyhtlKi2/xUzhc4RmFuy7rcu4kLSomyXEhOhTtcjXQ73j0VRizDKHplHBU5xECWcKVhV7C+XlcGA5SNhHhtLa7cM5kqNkm2/fAmQmWStSklWTWw0Jhuq8yl+UXLS1s7qCkB1t0JIO+qQLG1+Y3jxupJnJEtTKXETaW8rlmHMhI0uCRr/neGcwYWOzqEpdU2kE2AObtgpYAGg1hOmrsOrKw3MlSrC3BX2nmQIWEEthRFjaNGVw2I7W2y80pBBIhNhykS8jTuHKNpZLhUolI1KjzPbDxNtZknSFFOZysIAjJ6h8WPEs6m0KGWdNoVIZ0jsLYaKkuutoVvYqF/RFRWEJZ7o7Sx3GDy+wkXJWR2hCiPTaEkxW6fLgl19pFvrvIT7VCNsCzewoSxrHfB02iPTOPMPyxPEqlOSRyMyFH80GGyc6T6G2klmbW6PrMSjjoA8dI10MoTfgmnC7o6DXdFWT/S7TGCRmqKuerbTH6arxH57pukm7hDKT/P1NHsbvG1HVGRevCt3CC1OsI+e+0nxWBGbJ/pzGvCFKRbvffP5wAhhm+nGcueDOIbVv9701tPrUsmGUZPwbK5kat8tkybJmELPYjreyCnp1pLyCliYWkA3UGykDbttGQJvparc6kpEzX5hJ5IeSgehKPthrfxXiGdPUpFReJ5vvzCvYoCNhfkZUUzZMxXpZhJLoaasf4WZbQLem8NE3jyiy1+LVaS3bkXyo+oRkfiYpfSVpoco0But5lJt45yYt/COHpSdw7S5uYJM09LNreDKg2kOFIzAZLaXvCvTyU7e2rTX8osOU6QJFtCUVpdbkVgXs7LpCSN73Qm4HmiSUisYcq+VUnUm5sn+D8uUo+dClX9UQqodJ2HanQXJOpMTEuSElKJhoOIUQQUXKL2uQPoxG6fN4RxChCVyaZaYI0ZWrKQf4t7JPr8IyquPyGn0mam0mi9USdObXmErLNqPMtgE+e0LQAEgDQRSjVFcp3/lFdqsgexZUpI/oiyfVCxisYzk1fetQplVaG6XUALP4mW3nh89PdCT6Kcdi3oRvIu8Tziu0dItYlP8AzbDMxYbrk3Q758pA/SglXTDQvKFJcU+2pOhStgBST2HrxKo09iUaM0yy202tB6RaKyHS9h6+jzn5JP8AiR1+7Dh8buOn+ggf9yFTRnTm/BZoEegRWB6ZMPjdblvBr/FgpfTXhtAJW6pIHMln/FiiaEyZ8FqQLRUiunbCib3mx+Ox/jRz+7zhT/6m/wDzGP8AFhtAZUi3YEVF+71hTk8fyrH+LHK+n3CYFuIb/wA8z/iQbI2VIt+CJpF2lRUJ+EBhe+jn9a1+vHrnTfSH2rywXY7KztkfpQkrWGjSncuNA6ifCPbRleY6TawxMLbRUJhxkG4dSWQLd/U0PbrB0n0hYlmmUuNqqGVQBBuwoenJrAzFwU7Z8mnlMIVuI88nR2CM2jHlc+a5WplpY+chSWLj82Pfu6rX+/3/ADBn9WEzI8G7eXJpIMp7ICmkE6gRmgY+qineGMROqd2y3ZJ9GWDDjWuH/wB8m/MG/wBWNmR4D28uTSSm2zvaOOGyBbMmx13jNn3Y4pc1l5yovo+sHmh6imPG8YV43D1Un2ljdJcQbecJjOpHg2Q+TSgbZTqCn0iOXW2HRlUpsjaxIMZw+6+tDatT/wCUHugp/GlWatxa7OIJ2zzATf1QMaN275NINSkq0q6Q2D3Wg9aGjupIHjGaTiyuf74nz/z/ANkefdZWQdaxUP8AqD7oONcByHyaTKZYHVxsH+UIH3qNeI1+MIzSrFGJXD96TtQfTzJnyn1RyMV15JKJqo1FtQ3SJ5avYY2JGyHyaYUqVP8ACtfjCOSJUi4daP8ASEZqOKqkd6tUf+se98J04tnHrlus1BSQbH79e39MDEuDZD5NNEyoN+Mzf+WIIemJcC3Ha/HEZqXiSbPzqtUv+sdghVZqMycspPTjqxvnm3AIFxlR5Zo5+Zlv9u3fxvBjU9JsSflDsw0hkXusqAA1jK9XrNVkW0mbfcSHDZI8pcUT4C8Tah0+bxBKyz1VmXFsJT8nLg2QkeHb3798Zaahykye17pNQpZlMMyy5x5RsHiCEePafNp3wwol8W1FCnZuuzEpmP4Nnqj0Jtb0mJBSaVLSTYSw0lPbYakw5pyG4ScxG4QM1vG0FyY6hGOxCTg96ZsZ+r1B9R3u8qx8xJjmRwXR3kqcCHHsji2SVgXCkKKVDQdoMSXEqnRQp9Mo8uWfLRAeDZJbH0iBprlvbXQ6xXXR9JOYbrEwG5xx+XmUuB1kNgAqbUlKFXUs2NlGDFXTdzOTTsTZjCdMaACZdVuwurI9F7RGMb1DD1DLtKcpmaaflic6GUkJSvMm99ydD74njc4463mbZt/LWB7LxWHSZRKhU6oakz5KhtiTKFIzFSlqSoqA1SBYhR8479F1L0cGL17FXHD2GkG5FWdPetCBD5TsJYeeQgsy1PLyiU8ObqoBBvYAgbk9wI13EQt/EM1Lru7KSym1XyhI185/ZCNGJ55K1Ety5BGiUtJGXz2jojSqvd/2PLquki9El/5f/ZYj1Pw/KLeS2jDyi2glAQ2+8XFXICQQjLyuTe3ZeFLE3RUyjdk+TTWucy1JS4nc2y5nEm1rG517hFXN4kqiVqUp5KwfolsADwtY+uCU1qppcLnlbqifok3T6IPbye/+2L+o00tL/SRbNTqTSUqbpM1UX1hVg45JtMtlPMgdZXZa9vshnXUKoo9aZmE+DuT2RWwnZ4ul1Uy6pZ+sq49B0ghS3VvcZayXeSr2I8OyN2n8G/V7KyT+yxlzS1upQ/NFxSzYJU6VX9MW/wBHsy39zsgnit3SlSbFQ5KI2jLS1urdDq3FqdFrKKiSPPGkeh5al4Co+pNkuJ9Dq4SpRy1e5KXW9x6bbDt08NS9F6NZWoZS8pExLJU0DltdJtre3LsigG8fy6BY0x6388PdF8/CQYYb6OFmZLzbSp5gKs4T9Y2SkmwJtvaMsESyp1AQyoMgXUnPqrfnyisIQmr2Jd3XovDFlw4c6bZOSkmZWbkaonISMzbyXBYnQWVa1rxPqX0mSFSIMvK1ZRTur4sccA87WaMxTUolMxJLYuWXnAkX3CswuD6Rr3xp3oalvIaUgFWTiDPYne5hKsIQSaRej1taV1K30SeSxKmdKWxKzKgpQTmclXmfPZaBFX4/ojUxiN5xARmUu94vheYo3MV5iyVCqsVW3AiCYZyU3e1is2sOJsNrQoThpJ+rE4ZlhbQAQYtl4OM8BCFAr+VKjayLHbvvb1wtwEF+5lFtcsI5+lUaUk3TVpN95r6RaUR1dOxQMWcZbNyhkxZSgqgzyrahA3/lCDGTuFSUdbXKsT+50pwpFNnLjQg8XT8+DcnR7ypk15y7+vEYky2iqz7CmlKUhxWoA7YWhTJP4JXqjsat5ZBdT/hH6HtKOjznS5s+Bc/Xjo/udDalTR8S7+vDLZm34NQ8YLeLSFAIQVm2ttLRrfLB3H+Efoe3VdH/AAlhukTAcKTlUoOEA8vpxJMKYcl5mkSrzKRwlpJRZPK5iulutpGrB/G/ZGgejqSz4So7mQgLl0qA8YjW9K3HhVzHbCl/CI3VcOpYok8oJtlZUQbd0PXRZJrxFhx9tqWl5USNmAWwSHVZQSpQvvttEoxLJhGGqkojZhUGfBwlL4ZrKrbz1v6pHviG8QVHbUoauVh+dqzzrTTcoArIW2gCm6SRfUc4iKK7XXJ1xpE4UNcRSQssosACe7uiVvS2aemNN3l/pGISajKy77zbgdKkuLBypH1j3x10YrXQjXdktRdMVatsrzN1Bx1xQsS3LI5bAm0EfH2JD/6iYv8AzKP1Y4Fakh9B/wDEHvj347kfqTH4g98dGFcHNifJfPQl5XiahPMTgaS5JoTd1LfXcUsr+dy0y8gIhWJ6lOLrU+m6GTLuLYswnKk5CU5rG+ptFn/BWaS/S646kHKsS6k33seKYrXEcv8A6y169/39M/2io89pKbOyDvoQqQnMQTMul1yqvNBQBSC0kki2+0Kg9UkDNN1p1QBuNEIynt1SYg0s/LNLUJlrijL1QFFNj5oMW/KLbWlhnI5YkEqvbz3+yPRwR4OJVJJkierNSQpSUVx5ywGoSEj2R1TK3UnqhLNrqb7gUohSCQNMpPZ2iIlmV2n0w54WuuvygNzdStP6CoEoJRYYVG5I0V0RJfriJ2TfycOUQlYWE9dRWVbnut7IrTpHqVYcr7jMs8WVyzrsufJhkCkoVYKIvvrFy/B0lrzmIbjZtj2uRUHSPMNUvF9UefbcWhVQmkAIte/EvzPdHFRSzDqqPRkQMxiG5BnZy385+2DJZVZdURMVSbYG4JUTc+YwacTSPKVmPzffBbmIpNQ0lX/Sn3x3W+DlxfJxIzNTFSCJicmltEKCczp1000vE/6O3ZmZxLLUvMC3OFQWpQzKASgqsCdtUxA5KcbnKnLobbWnRS9SPqmLI6IWc3SbQgfrPD+ocjlrpHTSfpud9JrT0tW2aXmHk/UeSMovckp380WhhNpSacwlKUpsgfO19Q98QzpilsmOpe/Nls/nqifYeKUyreW56tuqLxzP2osiQsMZk2cUV6bHRPoG/nvCvIA3ZIGgtYQSwV2BS1vp1lW9l4WsMrUSFLtp9FNvbeEGGeqMceVWy4CULSUqFyLgixFxFD4Wp9YpfSVUJJxVQdp+VxTa3ypaSnOMpzEb7xpF6UKhbir/ABU/qwjdpqVKucyrdpt7LQ8Z4U1yLKKlZ8DbIsHydN94R1mn8aVfSE3JQoeOhiSNSSQAAlY7flV++PXpFK0EWUb6aqJhUxjE2I5fgty+nMj1CGS8TjpClQxLSptb5W35p90QY7mPUpu6POqK0jrMY6+hm7oLgxOremumsOxBbJstLSlTgzam4O0LpeXYNSQgstFBZUbFIIuCNYtjo26OaLWsKUSfm5d5b84lanLTBSDZxSRYAj6vZFSSaz8Yy5/4S7+owlWEo6vyPBpiyoS7KJRwttNpOVXzUgcovHoQQHOjymkJWSlx5Jsk/wC1Ud/PFOLli8yS4myMpsL/ADrgxePQWlP3ApQgZQ3OOpt2XCVf3o5Z3wHTT9499PDbf3GMJeyFtU83cKtyQ4dLpV2dkZbxPLts4gZQw23fgZiMqSkkqI2ygdnKNVdN8tNLwxTvi9jjuioIOVcwpGhbcTe9lc1DlGYaxT51nFglZmnoL6ZUKLKXOIkpVdQOYW5GPQ6TBLp8KfqucfUYlVxNaWElTpQlapQ21oZzvTEubtbEKWoaeOURqnAsmliiSByjrS6D6QIzCJZ8Ylw6h+WcYSZ5hCOIoKJCDsTfXeNb4cY4dFpg1FpVn9ARy9fFxkkzp6JqUW0KXWGwn8E3fvQIh+JJdPlaVAAeGkTeYSds5HgB7oieIWyXmyVG3m90cB2oZmWe/wBcHpZ13MdNj+MfV7oPQkk6FXmA90KY5Qz3QjxJLZsPzo7Up/STDo0nNqlZPhY/ZCWuJccpUyAsBjKByzLVmHdoB6z2W1ZboDM4yVLecr9XeHD4ZecSLq1uHCPshw+JX7nVjU6XX+yEFLlyvFNfWpoqT5Q6ASi4PyqvdDmJRJP72P5P9kdk3qca2DGqeEIyvJzqG/DsRYg63PPX1CCFUlbqi40WEoJIGZWVRsSCSOXm5WhyEraXR8idADbh90J3ZQrJJlXb2GzR90KpGaG+YojhTfjSYt2ufsjR2AJANYLoKCEKIkmrlOoPVB0jPcxT3AybS7pJGwbPujTeApYt4Hw8lWZKhTpfQgAj5NMSrvRFaKs2JsXSwGFqoSkaMHlHXwcW8uEamq3zqgr+ybhZjVq2Eauc5/e59ojz4PSMuDJ3vqCz/VNQlJYtA9Q/SZ8EvmmnDY6uKPriq5iizMxOTDqHJZKFurIzugG2YxdUvL5nVHfrn2xUTtGmHTMPtSxWXHVKFhqRmMdPTvcn1CukeM0ZhDGV9aeIkEKU2UqTewsRc3Pq3MFKw4tay55XIpSdQnjbDsvBy6NUigZJFwnXS6ffCVdIqjSQZiSLbf1itJJ9cdN2c1lsak+CYUimVxlOUlpMsklBuk/hdogNfQ2/ibEBbIVlqE0g2vuHVXETz4I6Upp+IU2IcT5MlYsLC3F2POK0Uh5vpBxi2pd5ddSnXEpB+lx1DTzb+Eck4+Tppv1tFXU2qVNqVal5NbZSnRILKVHU9viY4qVWqc1JrZm1NFlaQrRoJJ0ChrbwPnhJR55uTmiqaC1MFGgDYcsrSxAJHfz5x1U51iadUJVC0MhASlJbCNbamwJ38eQjsORtDWDcGHnBic2JZAf8Q/oKhntaH7A+uK6cDzcP6Ko0/azU16kav+DwyG53EA7W2Pa5FEdLi5ecxfWZVS0tLZqsyFKWFZbcQjkDF/8AQAkipV7rDKWmNLa3zOa3jP8A0oUp6ax/icSKC5MGqzGZpVk2BUo5gSRHHRWqZ1VH6pIjEtRZViYUuanZB5oFIyoc15bXAHjflfnHk9TJOYQOBOSUssAEqffTrqdAU3v337oCML1+xSuT4iCLFJfat4/OgheFa5lsuU0Ha+2f70dmpzOwqotPSxWZMidlJgkKTlZczH5ijfw0i1+iVvL0m0D+dcH9SuKuw9Rp2Rr1PXNshCDmbuFpOuRXYT2RbvRg3k6SKCrsfUP6tQjkr+46qXsY9dNjATjaRJG7Kf7QxNKEj5FHhEa6dWh91lOXr+BtobfT/bEnpDSCwjMnNcczeOV7IvDZD80CEpJG5hxl1t5jdaNuahDdLMN2BDTf4oh2k05SAnTTlCBZ6XWRu63f+UI5C2Lk8ZrX+OIXJBta5j23ngiYmIOKx/tW/MoGOszRUmyhvyELCm51MeFIunxjWDdmPultkIpUsQRcTQH5q4rZWRUoEo1duSRYC3eSe7l3xcPTU1loCSPoz6R+asRTaEIJKi4EkHbSPTov0nJWXqD25dppY8qPVUABk0vpreFbzct8XqWylAOUAEb7iG5zLpd0rPpt64DRQGvwqyTunLoR43+yKMiiaYTxdiSnU6SZlK1NMSsiViVabCcqMxJVcZddSd77w2sOyrNTlVFGVz5QAjbbn6IZJUNhJ6xve+9o6UvLOMEa2Uba3vdPbCybe4y0RMZiYQWQQQQrYxcXwfl8TBlQRkWrJU16i1tWWdNTGdXZh4IACbC/JV4v/wCC89xcIVtCjqipZ/xmUD+7HPV/bZen70TfpfnXJKn0Z4U+pzSUTucpkZYPKT1SASb2QCVAXPb4GM/4lmFz/S+88hlySWmntgtr+UU3ZtKTm4ebfuvvGgOmmYal6NJJeLwCluLHCd4WbKBpmv37ak2jOCm2nek2ZSpgMt+QNHhKPEyXQ2SL8zrvHb+Ns5JfJHrk8rE/kUVNK1YxwkhT3FSZorFkrAFgL/OSm/beNP8AlUvSqNJvz8zJSksllpHFmZkNJvlFhdQtfTa8ZmdSlzpCwulKswzzCyrKASQ1fXt2jTeIcnxPLIBBsUDTuSYT8u7dR9B/G/sjYrE9FdJDdao7iuxuebWfVDJiCotKKS2S5Y65G1qNvAJjx2VlnPwrDK+3MgGI1jebl6BhaoPU6WlmJh0Jb4iGQCEk2Va1tbE68o8u6fg9DVAdxZSZYuB2aBUhGeyUKOYWO2mu0cYhqCJiUlvJX0FLiyQq1xYBVyUH53zSBfS9t4o2Wqa1zbhdccLS0nMlJFzoRvr2mJMxVmFkLazlmXaUEHQnYkgXOoBJ03uTzsInUxLSwuJSWjLCLzjaHpV51PFYSpRUghai5mtlBAAJvcbdluUKqtOtyzzMoioLmC2lKVslwakhJzg899QdtNoruSrE0/NKdJKnHFIc4TY53zXOmouIIqM8+xTGWnUuocbK1cVJGhUewDT6Mc8YvFY0pRtoLA1KtonpqTng6OMpx1ktqStvMtW47L3F4LaqjF0jjb9xg/CGHpaq8ZU7WEyLilhCWkAL4gN9zcDUg9W5Oh0HNyqeF5ChKC36iZxhYyJLaeG42s3sdyMu1z3x3qrgheWpLBid9hEqaS0n5RWXwuf8nugpVWlgSC/t/FV7oSuFEvUWZV558sKaS2H0klC7qva+gGwJ19NoXSsnRkkmYmHnXEqusHrIFtxmFiDqNzudrWMTj1bivUhp04X9H9hDlXlEpOZ8/iK90aPwgorwnQlocRlVIMKF0HYtp74zU8xS5hxDIXMJltPvhJFkG5ugg3NvHW+19ov/AAG+4xhySan3A3wkBpltRAUGkISlOYGxCrAE95PgFqdTGaSeg1OGG7uOeOCsYOrBKkfvdX0T3d8G9AQtgl86HNOrOn8hEN0/OHGGGaxT6IkKmXEFtBUsAAXFirmOfIxM8PvP0uhykkaRNZ2mkoWththtKlAAEhPE7ofpK1OSxJ6Eq7vGyM408BeU30vyir5XEUnJtZHkP5m1KQSlKSCQeWsaIxrR6jMYvmnmKe62062hxN2kpSkBITY5SRmukn0RCB0byCkrccoLCkjrKWXHdzvzi1OpGFw1IuokV2rFlOAScswq4vYJGnrhO7jCQWkoMpNG/aEe+LWkejCSm5tqWaw5LFxwEpzqWnYX+kRDsroTJ+bh2SRfS5WDb8+KvqobEslryiSfBZWh2h1Z5F/lSwux3F+IbRWD7gVjnFzKUkBFSmnCq97lTy+7ujReApKawrhSUpDlLfUJUKALJaSi2YkAde+x3Ot4iOM8C1jEOK5qsNBbTb0s3LoaWG8yAnMdSFa3KieZiU6kbKzGpNKTuY/p1f8AI5Jtj4opMwWwflH5XOpWt9Tfvjqfr5n5JyWNJo8sF2PFlpbI4mxvob91ouhHwcarwxlmXCLc+H747/0b6plNpnfQ/MEX7mnyQwMzxqToLw+4H0xZS+V3rfmmLm/0camDYKWTvopH60HUnoCrFNq8pNoTMZmV8QDM0Qbcvn98aXU02mrhjTakmWN0AuOfdHiBkqu15LLrSLag53AdfRFP9KVcXTse4hdmG+MxL1RbSG0AJVdYUom/mjSuAWKlh2ht02dpkw4UuLUlxstAWUb69e5N7+a0RrpXwlN4xnZN00xpcswmwbmW2yc5Ns1wrXfY7a9sQjVhGKdyz9U5LkzSMfywTf4umLdzg90EuY8llmwp74O2rg90W9NdFbMoyHJmm0hps/SUGwPSVQwVDC1DkXuHNSFPQ5e1uECdxtbxhZ/kqFPdiOjbyQGlV9qqV2mMIl1tKLxVdSgfoKH2xaXR2MnSJQrneZ/uqhLRsH0ubmEOUqXpyJptWigEIUk7aXI7bensiyej2h1LD07UHX2Zh2XmWg0tMuprMCDe9yrsJGnb3CJ93SrWlFloRag0tRs6fepXqWu4ALSt+5aYkNKUvgt6psUg3yn3ws6R3Jmv4cdpsjJTss+5lIK0MqDliDlUSu6Re2sNlJU83JS6ZhlaHUoyrSSkkEacjbW19O2FbT2ZSne2qJFLKXp1kfi/th0lnVlRSEJJA5qt9kMkq8nQkkDtUkgeuHWnKzu5knMmx1G0KO0OIcet8xr8of1YHEe/2bX5U/qx7HohriYTniP822fyp/VjzM6SOo1+UP6sdmOY1zYTNHTe2Rh6dBAHDnknQ/xlDs74o5tsKQtfDcURa3Yb+aL/AOnVq2HK4QNEzaT4fLgfbFAB0hKU5Vmwjvov0nJWXqOm2w4bBtSFfxk6H1aR00JhqzgaSAjra5VDTXUHfwIgsvr5JXBBU5ckrXrvrFrkrCyTUlQKVApUToqwt4QYpDgeb+QWoJXyTcbHuhGw8W0WTnIvyjlb76ldVxaQNQID3CthzdXkF1N5fEW+yL2+C3MrNKxM0G8wQ/LL3AtdLg/uxnRx59xNl2J+tbU/ZF+/BPcObFragQbSa/7YfbEa0bQZWm7yRZfTfRsZVZqhMYHlGnsin1TSnSEhN+Hk1PPRe0VTK9CXSbNVh6qzUzRpOcfQELcU+VkAWtplIv1Rzi9KbjeZmqw1JLcpKAtt1SlsurdDRRl0UDlsTm7eUdTmKFIdUDU1LTY/vVgZR6l+2ILrp0UoxG7Z1Hqir6B0HYmlq/JVmvYmp8yuUDmVpDKrnOgoPWFuRvtyiXdIdeepwZkkILjoZSttDaFOAkJdGqrC2ob380LHsTFwkBNQe71zSmQfMg+0Q3GZnZhJIakUJ166kcRze/zlk39Ec9TqpVZYp6nVR6Z01ZETmsQvpEwWpeacSm/DySjhKz19tOXV374hnSHVKhU5MS0tTqgpq5UFeSOJv1lW0IuLWT/kxZ5lVpUr74dKlKKlBK0gXO/cIVokZgtngz7zZVsG3jf3RlWitbFJUZPS5l9in15QKE0efsFlYLcm5muQR2Wh7pOFcXPBvg4YqhWm5St2VUgKUTqSTYbX11FzGhVYdmXgM9SnnFHYCacPqFvbBTmDnnCC/MuIVtnWArTxIvGl1UHpYn2r8so+s0HGNBl3KpWJFUkgu8POXmlGygbAhKifSNIJl5CrYil1yMuuWMwU8VRcWEXFwDdR2NrDTsi66l0ay1Ulky0zWHUtJWHAGkNi6he17pN9+yPaZ0Y06lzjT7dQnlrb1tmaSgnvCUpv5xE8+G63OiFKMYOLK6w1hKuSMtLstCnkJe4l1TqQRoNNQOz1mFNfwhiKqOMcNdPSUXuFVNAF7kg6K3sfVF0ytCbTbI4b9xsR6NIVGkICsxUsrvuXLmB3Lfgn28bWuZ5Z6NMSMTS1KFLWRoHE1Brr8rgX05cuUev4GxS0240j4vSlWikips6pB0Fs19O4xolumEHrFKkjUC3+RHq5RtStUrSoc+KBb0KjdzfwDto8mcZfAGJZItTShIsoaIAcRMtKKOWYgXJF7HW8Lqu5V6gtpD9TCni2kNJUrlbTUAAHX1Rd9Yw6mqGVadcW7LIXxXEhz55TbKCTuL6+bwip8YVRLdTqVJdp7a0y76kBxSblQudzyGlu60cdfMrTWGKdv6I1KOHZ6HHR7ildImp0uS781LhNlolycySOdhrYWIJI0uYtLD9dl683LJptNcmHnlBCkGprQpsb5jzKQL8uUUdSaGarRKlNC4U2tRbUpxSlDKNzzBuL3i+8P0JimeTvSD3CaVLpTcEoVcgb23ufC3nhJdPKhUjKN7N6oNOGJWJojD7bQLaSFt5cqc5Uo+JJOsEtYcZCweG2kdyPZ2Q1KU8nRyZeSNgQ4rX1iPAHFoANQmmk8ghRJ897iO51o8B7edvcNlfm5WQxCadKSMhM8Fm8x5RMKYGdViLFKVcvbBXlnWQkUKiAk7CovH/sQr8jU2tSlOLWgnQOKA181gYMUylCQtSkZj3A2PjeElVvsi0enVtXqIZmYUpsheH6Ab7f+IuX/sI6TO8UqSrDdBy238tWbns/A29cK0tJVr85ZGl0g29ptHKpZaVBKXFi51ulVv0YVVGF9PDkSeUsoQkCh4aKkgJOedWTfzMwT5apIF8O4X3I6824OZ2uxDk6wnMhAUkc+YIt4wU5JMF0BZNrXs4bj7YbN+AdvAaV1VxK1AYYwoRYWPlp137We6ODUFFQJwrhXKAdRPEjl/wIdltSrThSt0pJTcZVFI+yC2paXccWCVk6G+e979up7IGb8B7aIzvzybpvhTDxsoEZaioDf+Z74OnJlcu3ZeHMPs8RviJAqDlyk7EfI90PbUi0rMnK6rL9TXTlBD0hc2sEqTsVpAJHojZielgZCvuVti6enOIU09cnKuuLBysLU4FpA+bYNg3ub3vy21itqzPznljTj6HFE26kukqsN8wIGl7W2NvbbuN6G2KTV51qaDT4ZAKR9IXF7Hst/nSKypsg2ykKQSNQDsTv/ke+F6agk25I56lL1EowaKROya5ha35ebP74C05FKXzUkKQBYncDaHmWq7Uo46l2iS08ki2dc6GlE9tshAEeYQorbNQmlNTKXUqay2J1tmHdf0j2xKW6YpSrN7p+utIPmvC9vTpVHNeS1OksNyJTVbkJgJBws0nrC4VWVIHZyb74c8G1FtYfk2JCXkWmSFoQ1OGYzZib3JSLWI9cPjtJz6LQFEfXWAfMb6wlXREKVZKGioajq2WPA846I1Eh3TZJJR0WHph2kEtuTXyiErJB+ckGInLNT7CghmaS92tPoUTbuUNYeqbUCw6kzsu4wbEC9rHzkiKYkwODJKJdkbNIHgLR6WGuwjwURDcmtShUE8RQWeXDUfYLQaupyrSc0w+GUbZnEqQPSQIKZPB8Czgt/Wd/LL98DgI3u5+VX74RCrSChdM9KEd7qR9sGJqcilxKXJyVbzajO8gX9Jg6gaSKS6dpcJwviG1z8qlWpJ/h0mM6oCcibqN7DnGxsR0pqu1dcsZH4ypzzuWYyLBQU2vrZWuoA07Y4mehvBDsq4v7lS26Emwbef38y46adZQVmmc9SN3dMx9kHIKN+wwFsqym6XNu2NcP9AeBnb2ptSY725pz7bw1zXweMGlxCWZmtslZIt5QhVrAnmjuh11MPNyWFmVJVtTiFZQo2Oto7LauxUace+DhQAn73r1Ya56toV9ghrmvg5y4cCJbFk0kqSVDiyg2FhyV3we4pvyBQZnUtm9+sIuz4LDq26/iNpCUqC5NpZzrKfmuEdh+tCib+DzUGtWMWyiv51hafYTEm6GsAVHBmLag5P1SSnm5iQUgCXz3BDqDc5gO+BOrCUWkylOElJNok9UmxNol2USspKS7Fy21KtBpIJtfQeEJQ02U51pUsjkpaj9sCBHn13ebueh06/40KpFlt+5yFvLtkWr7SYWp6qNADba4vAgRBlxybl2wyHCMxPI7eqDOqlOZLbYtpoLQIEKZnrYStRBQkeFxCpLCEISsX6xAsdRAgRhWHKShLZPDSSBfmL+iDilKWsyUgabQIEZIhJu4WboZLgOptpYW180eOkJbBKEk+FvZAgQQrYNTLJLIczLBIGmloJmmEsU5x+5WtKCsBWguB3WgQIyNdkaqFVmGZfiJyEkXsq5A814piaX8Z4jrLs2lKnFPKJIFvpGBAjpoLcStsg1x5dJpigwUuJQ2dHEix6x7Leq0WnhGtz07TpRx925LSbADQaDQQIEav7Q0tyTysw5MISt0gqOu20K2GkrCVkqCiLXCrQIEcUtiz0R2yy2s3cClm5AzLUba27YPblGgtSgCDe3zibemBAjnlJpPUjid9wxKEocUdSdBudBbugh0JU5nKU3tba/tgQI45VZ8sVPUTrAz5wlN7W2jwOlHWCEXIA+bAgRHNqX9z+yz2OFO5lgqbbVbtTeAHMqgpKEJNrXSLQIEDNnf3P7MGqcKspVqR4wRNybMykF0KuNiFkH2wIEI61T/ALP7AmMdQwbSqiC3N+VLQsWID6hv2EaiGsdEWHkIuiarKU/U8tUR6xAgRSPU1ktJv7YJpNjjSOjqh018qaVPuqXpd6bWvL4C9h42iRy+HpJKQn5VQH1l3gQI7IVqjWsn9iPTYOXTZYIKMqtCRmzEHYQndQWnOGlaiALgm1xAgR2Rk77lKbb3CruBxtsvvKSq5N1mE86+4y622ggpUbHOkLPpN4ECLxbaKJK5wpakqGYhYUbZVAWHotCafShlpTiGm9Po5dDAgQyCcIQkpz5U6i9raQgckpSbCi7LNA3t1QRz7IECGTFYyzVIp7iVXlUAg2BClC3rhBL0KWcUtPFmEpvslYF/VAgRaO5NrUKqbD9NDYk6jUGr3PVmCPZCFOJK8yrK3XKoAna8yo+0wIENdi4U/A5s4xxIltWStTQI5qCF/pJMHs47xAW3FvTbLyk3AK5VrbzJECBGuTlCPAoR0h1V1j5aUprl+rqyoexQhZR6+58Zqm0Scqh8y6wSC6QRobWKyNwNoECAtxdj/9k=');
                        background-size: 98% auto;
                        background-repeat: no-repeat;
                      }}
                      html{{
                        margin:10px;
                        padding:10px;
                      }}
                    </style>
                  </head>
                  <body>
                    <header>
                      <table>
                        <tr>
                          <td width=""90%"" height=""150px"" class=""fondo-hotel""></td>

                          <td width=""10%"" height=""150px"">
                            
                            <br><b>Reserva: {reservation.Id}</b>
                          </td>
                        </tr>
                      </table>
                      <hr>
                    </header>
                    <main>
                      <table>
                        <tr>
                          <td width=""20%"" class=""verticalline"">
                            <font face=""verdana"">
                              <h1>HOTEL REAL</h1>
                            </font>

                          </td>
                          <td width=""25%"">
                            <p class=""letrica"">
                              <b>Dirección:</b>
                              Carretera A Masaya Km 4.5, frente a TGI Friday's. Managua, Nicaragua
                              <br><b>CP:</b> 14031<br>
                              <b>Teléfono:</b>
                              +505 2277-4548
                              <br>
                              <b>RUC:</b>
                              881-002890000U
                              <br>
                              <b>DGME ROC:</b> 6363512363512
                              <br>
                              <b>LICENCIA INTUR:</b> 170122-H-19870
                              <b>Check In:</b> 14:00 <br> <b>Check Out:</b> 10:00
                            </p>
                          </td>
                          <td width=""20%"" class=""centrar"">
                            <div class=""letrica"">Entrada</div>
                            <h3>{reservation.DateIn.ToString("dd-MM-yyyy")}</h3>
                            <h5>{reservation.DateIn.ToString("hh:mm tt")}</h5>
                          </td>
                          <td width=""20%"" class=""centrar"">
                            <div class=""letrica"">Salida</div>

                            <h3>{reservation.DateOut.ToString("dd-MM-yyyy")}</h3>
                            <h5>{reservation.DateOut.ToString("hh:mm tt")}</h5>
                          </td>
                          <td width=""15%"" class=""centrar"">
                            <h1>{reservation.TotalNights}</h1>
                            <h4>NOCHES</h4>
                            <h5>{reservation.PaymentNights} pagadas</h5>
                          </td>
                        </tr>
                      </table>
                      <hr>
                      <table class=""letrota"">
                        <tr>
                          <th colspan=""4"">HUESPEDES</th>
                          <th colspan=""4"" class=""centrar"">FACTURACION</th>
                        </tr>
                          <tr>
                            <th width=""30%"">Nombre y apellidos</th>
                            <th width=""10%"">Pasaporte</th>
                            <th width=""10%"">Aerolinea</th>
                            <th width=""10%"">No. Vuelo</th>
                            <th width=""10%"" class=""centrar"">Transporte</th>
                            <th width=""10%"" class=""centrar"">Hospedaje</th>
                            <th width=""10%"" class=""centrar"">Descuento</th>
                            <th width=""10%"" class=""centrar"">Subtotal</th>
                          </tr>";

			foreach (ClientOutput client in reservation.Clients.Where(x=>x.Age=="adult"))
			{
				html += $@"<tr>
                            <td width=""30%"">{client.Name}</td>
                            <td width=""10%"">{client.Passport}</td>
                            <td width=""10%"">No data</td>
                            <td width=""10%"">No data</td>
                            <td width=""10%"" class=""centrar"">+ ${reservation.Transport}.00</td>
                            <td width=""10%"" class=""centrar"">+ ${reservation.CostPerClient}.00</td>
                            <td width=""10%"" class=""centrar"">- ${reservation.Discount}.00</td>
                            <td width=""10%"" class=""centrar""><b>${reservation.TotalPaydedPerClient}.00</b></td>
                        </tr>";

			}
            if (reservation.Clients.Where(x => x.Age == "minor").Count()>0)
            {
                html += $@"<tr>
                              <th colspan=""9"">MENORES DE 2 AÑOS (Los menores de 2 años no pagan)</th>
                           </tr>";
            }
			foreach (ClientOutput client in reservation.Clients.Where(x => x.Age == "minor"))
			{
				html += $@"<tr>
                            <td width=""30%"">{client.Name}</td>
                            <td width=""10%"">{client.Passport}</td>
                            <td width=""10%"">No data</td>
                            <td width=""10%"">No data</td>
                            <td width=""10%"" class=""centrar"">+ $ 00.00</td>
                            <td width=""10%"" class=""centrar"">+ $ 00.00</td>
                            <td width=""10%"" class=""centrar"">- $ 00.00</td>
                            <td width=""10%"" class=""centrar""><b>$ 00.00</b></td>
                        </tr>";

			}

			html += $@"	<tr>
							<td colspan=""3"">
							<p class=""letrica""><b>Gestor: </b>{reservation.UserEmail}<br>
							<b>Fecha de emisión: </b>{reservation.CreatedTime}<br>
							</p>
							</td>
							<td colspan=""3"" class=""derecha"">
							<h2>TOTAL</h2>
							</td>
							<td colspan=""3"" class=""centrar"">
							<h2>$ {reservation.TotalPayded}.00 </h2>
            
							</td>
						</tr>
						</table>
			";

			
			 html += $@" <hr>
						  <table class=""letrica"">
							<tr>
							  <td width=""45%"">
								<p>
								  <b>El precio final que se muestra es el importe que pagarás al alojamiento.</b><br>
								  No se cobra a los clientes cargos de gestión, de administración ni de cualquier otro tipo.
								  <br>
								  <br>
								  <b>Información sobre el pago:</b><br>
								  Hotel Real gestiona todos los pagos.<br>
								  Este alojamiento acepta las siguientes formas de pago: Efectivo (Transferencias solo con su Gestor).<br>
								  Niños de 2 años en adelante pagan.
								</p>
							  </td>
							  <td width=""55%"">
								<p>
								  <b>Información adicional</b><br>
								  Los suplementos adicionales (como cama supletoria, retractilado y preparación de equipajes, alimentos y bebidas extras) no están incluidos en el precio total. La reservación es de carácter personal, e intransferible. El pago por adelantado no es reembolsable.<br>
								  En caso de no poder viajar en las fechas dadas originalmente, su pago por adelantado es válido para reprogramación solo antes de las 72 horas de su Check-in en el Hotel y tendrá un costo adicional de $ 40.00 USD por huésped. Recuerde leer la información importante que aparece a continuación.
								</p>
							  </td>
							</tr>
							<tr>
							  <td colspan=""2"">
								  <p class=""letrica"">
										• Es obligación única del cliente informar a su Gestor de reservas Hotel Real, Managua, Nicaragua que no podrá hospedarse en las fechas acordadas.<br>
									• Solo se aceptarán re-programaciones si se informa de las mismas hasta 72 horas antes del vuelo que no efectuará.<br>
										• Si el cliente luego de arribar a Nicaragua NO decide hospedarse, el pago por adelantado no es reembolsable. Igualmente tampoco es reembolsable el costo adicional por reprogramación.<br>
									• Si el cliente decide hospedarse en el Hotel debe completar el pago total de la reservación indicado en su Factura a partir de la tercera noche una vez que realice el  Check-in en el Hotel.
									</p>
							  </td>
							</tr>
						  </table>
						  <hr>
						  <table class=""letrica"">
							<tr>
							  <td width=""75%"" class=""verticalline"">
								<p>
								  <b>Habitación climatizada con baño privado interior.</b>
								  <br><br>
								  <b>Régimen de comidas:</b><br>
								  Desayuno, cena y café cubano 24/7 incluido en el precio de la reserva.
								  <br><br>
								  • Transporte Aeropuerto-Hotel •Piscina • Gimnasio • Artículos de aseo gratuitos • Ducha • Aire acondicionado • Toallas • Ropa de cama • Enchufe cerca de la cama • Escritorio • Zona de estar • TV • TV de pantalla plana • Canales por cable • Perchero • Papel higiénico • Aire acondicionado individual para cada alojamiento • Desinfectante de manos. • Kit de mascarilla • WiFi
								</p>
							  </td>
							  <td width=""25%"">
								<b>Pago por adelantado: 
								</b><br>
								descuento en facturación.
							  </td>
							</tr>
							<tr>
							  <td colspan=""2"">
								<p>
								  Hotel Real, Managua, Nicaragua no emite reservas a personas que no hayan entrado legalmente a Nicaragua. Cada reserva será siempre procesada contra pasaporte con cuño de entrada en la fecha de entrada que refleja el boleto. 
								</p>
							  </td>
							</tr>
						  </table>
						</main>
					  </body>

					</html>
			";
	
			return html;
		}
	}
}
