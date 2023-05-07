using HotelAPI2.Common;
using HotelAPI2.Domain;
using HotelAPI2.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace HotelAPI2.Repositories
{
	public class ClientRepository
	{
		
		public Response<Client> AddClient(Client res, HotelContext hc)
		{
			Response<Client> result = new Response<Client>();
			
			hc.Clients.Add(res);
			if (hc.SaveChangesAsync().Result != 0)
			{
				result.Success = res;
			}
			else
			{
				result.Error = true;
				result.Message = "Error in operation";
			}

			return result;
		}


		public Response<Client> GetOne(int id, HotelContext hc)
		{
			Response<Client> result = new Response<Client>();
			var client = hc.Clients.FindAsync(id).Result;
			if (client is Client)
			{
				result.Success = client;
			}
			else
			{
				result.Error = true;
				result.Message = "Error in operation";
			}
			return result;
		}

		public Response<Client> GetOneByPassport(string passport, HotelContext hc)
		{
			Response<Client> result = new Response<Client>();
			var client = hc.Clients.Where(c => c.Passport == passport).FirstOrDefaultAsync().Result;
			if (client is Client)
			{
				result.Success = client;
			}
			else
			{
				result.Error = true;
				result.Message = "Error in operation";
			}
			return result;
		}

		public Response<bool> Remove(int id, HotelContext hc)
		{
			Response<bool> result = new Response<bool>();
			var client = hc.Clients.FindAsync(id).Result;
			if (client is Client)
			{
				hc.Clients.Remove(client);

				if (hc.SaveChangesAsync().Result != 0)
				{
					result.Success = true;
				}
				else
				{
					result.Error = true;
					result.Message = "Error in operation";
				}

			}
			else
			{
				result.Error = true;
				result.Message = "Error in operation";
			}

			return result;
		}

		public Response<Client> Edit(Client res, int id,string email, HotelContext hc)
		{
			Response<Client> result = new Response<Client>();
			var client = hc.Clients.FindAsync(id).Result;
			if (client is Client)
			{
				client.Age = res.Age;
				client.Email = res.Email;
				client.Name = res.Name;
				client.Passport = res.Passport;
				client.Phone = res.Phone;
				client.Reservations = res.Reservations;
				
				client.UpdatedTime = DateTime.UtcNow;
				client.UpdatedBy = email;

				if (hc.SaveChangesAsync().Result != 0)
				{
					result.Success = client;
				}
				else
				{
					result.Error = true;
					result.Message = "Error in operation";
				}

			}
			else
			{
				result.Error = true;
				result.Message = "Error in operation";
			}

			return result;
		}

		public Response<List<Client>> GetAll(HotelContext hc)
		{
			Response<List<Client>> result = new Response<List<Client>>();
			var clients = hc.Clients.ToListAsync().Result;
			if (clients is List<Client>)
			{
				result.Success = clients;
			}
			else
			{
				result.Error = true;
				result.Message = "Error in operation";
			}
			return result;
		}
	}
}
