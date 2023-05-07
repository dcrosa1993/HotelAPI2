using HotelAPI2.Common;
using HotelAPI2.Domain;
using HotelAPI2.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotelAPI2.Repositories
{
	public class UserRepository
	{
		
		public Response<UserOutput> AddUser(UserInput res,string email, HotelContext hc, Mappers mappers)
		{
			Response<UserOutput> result = new Response<UserOutput>();
			User user = new User()
			{
				Name = res.Name,
				Email = res.Email,
				Baned = res.Baned,
				Phone = res.Phone,
				Password = res.Password,
				Role = res.Role,
				ChangePassword = true,
				CreatedBy = email,
				UpdatedBy= email,
				UpdatedTime= DateTime.UtcNow
			};
			hc.Users.Add(user);
			if (hc.SaveChangesAsync().Result != 0)
			{
				result.Success = mappers.mapUser(user);
			}
			else
			{
				result.Error = true;
				result.Message = "Error in operation";
			}

			return result;
		}


		public Response<UserOutput> GetOne(int id, HotelContext hc, Mappers mappers)
		{
			Response<UserOutput> result = new Response<UserOutput>();
			var user = hc.Users.Include("Reservations").Where(x => x.Id == id).FirstOrDefaultAsync().Result;
			if (user is User)
			{
				result.Success = mappers.mapUser(user);
			}
			else
			{
				result.Error = true;
				result.Message = "Error in operation";
			}
			return result;
		}

		public Response<UserOutput> GetUserByEmail(Credentials credentials, HotelContext hc, Mappers mappers)
		{
			Response<UserOutput> result = new Response<UserOutput>();
			var user = hc.Users.Include("Reservations").Where(x => x.Email == credentials.Email).FirstOrDefaultAsync().Result;
			if (user is User && user.Password == credentials.Password)
			{
				result.Success = mappers.mapUser(user);
			}
			else
			{
				result.Error = true;
				result.Message = "User or password incorrect";
			}
			return result;
		}

		public Response<User> GetUserByEmail(string email, HotelContext hc, Mappers mappers)
		{
			Response<User> result = new Response<User>();
			var user = hc.Users.Include("Reservations").Where(u => u.Email == email).FirstOrDefaultAsync().Result;
			if (user is User)
			{
				result.Success = user;
			}
			else
			{
				result.Error = true;
				result.Message = "User or password incorrect";
			}
			return result;
		}

		public Response<bool> Remove(int id, HotelContext hc)
		{
			Response<bool> result = new Response<bool>();
			var user = hc.Users.Include("Reservations").Where(x => x.Id == id).FirstOrDefaultAsync().Result;
			if (user is User)
			{
				hc.Users.Remove(user);

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

		public Response<UserOutput> Edit(UserInput res, int id,string email, HotelContext hc, Mappers mappers)
		{
			Response<UserOutput> result = new Response<UserOutput>();
			var user = hc.Users.Include("Reservations").Where(x => x.Id == id).FirstOrDefaultAsync().Result;
			if (user is User)
			{
				user.Email = res.Email;
				user.Name = res.Name;
				user.UpdatedTime = DateTime.UtcNow;
				user.Baned = res.Baned;
				user.Phone = res.Phone;
				user.Role = res.Role;
				user.ChangePassword = res.ChangePassword;
				user.UpdatedBy = email;

				if (hc.SaveChangesAsync().Result != 0)
				{
					result.Success = mappers.mapUser(user);
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

		public Response<List<UserOutput>> GetAll(HotelContext hc, Mappers mappers)
		{
			Response<List<UserOutput>> result = new Response<List<UserOutput>>();
			var users = hc.Users.Include("Reservations").ToListAsync().Result;
			if (users is List<User>)
			{
				List<UserOutput> list = new List<UserOutput>();
				users.ForEach((user) => list.Add(mappers.mapUser(user)));
				result.Success = list;
			}
			else
			{
				result.Error = true;
				result.Message = "Error in operation";
			}
			return result;
		}

		public IResult Login(Credentials credentials, HotelContext hc, IConfiguration configuration, Mappers mappers)
		{
			Response<UserOutput> response = this.GetUserByEmail(credentials, hc, mappers);
			if (response.Error)
				return Results.Problem(response.Message);


			var claims = new[]
			{
				new Claim(ClaimTypes.NameIdentifier, response.Success.Name),
				new Claim(ClaimTypes.Email, response.Success.Email),
				new Claim(ClaimTypes.Role, response.Success.Role),
			};
			var token = new JwtSecurityToken
				(
					issuer: configuration["Jwt:Issuer"],
					audience: configuration["Jwt:Audience"],
					claims: claims,
					expires: DateTime.UtcNow.AddDays(1),
					notBefore: DateTime.UtcNow,
					signingCredentials: new SigningCredentials
					(
						new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
						SecurityAlgorithms.HmacSha256
					)
				);
			var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
			return Results.Ok(tokenString);
		}
	}
}
