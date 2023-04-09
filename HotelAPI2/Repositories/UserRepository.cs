using HotelAPI2.Common;
using HotelAPI2.Domain;
using HotelAPI2.DTOs;
using Microsoft.EntityFrameworkCore;

namespace HotelAPI2.Repositories
{
	public class UserRepository
	{
		
		public Response<User> AddUser(UserInput res, HotelContext hc)
		{
			Response<User> result = new Response<User>();
			User user = new User()
			{
				Name = res.Name,
				Email = res.Email,
				Baned = res.Baned,
				Phone = res.Phone,
				Password = res.Password,
				Role = res.Role,
				ChangePassword = true,
				CreatedBy = 0,
				UpdatedBy= 0,
				UpdatedTime= DateTime.UtcNow
				
			};
			hc.Users.Add(user);
			if (hc.SaveChangesAsync().Result != 0)
			{
				user.Password = "";
				result.Success = user;
			}
			else
			{
				result.Error = true;
				result.Message = "Error in operation";
			}

			return result;
		}


		public Response<User> GetOne(int id, HotelContext hc)
		{
			Response<User> result = new Response<User>();
			var user = hc.Users.FindAsync(id).Result;
			if (user is User)
			{
				user.Password = "";
				result.Success = user;
			}
			else
			{
				result.Error = true;
				result.Message = "Error in operation";
			}
			return result;
		}

		public Response<User> GetUserByEmail(Credentials credentials, HotelContext hc)
		{
			Response<User> result = new Response<User>();
			var user = hc.Users.Where(u => u.Email == credentials.email).FirstOrDefaultAsync().Result;

			if (user is User && user.Password == credentials.password)
			{
				user.Password = "";
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
			var user = hc.Users.FindAsync(id).Result;
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

		public Response<User> Edit(UserInput res, int id, HotelContext hc)
		{
			Response<User> result = new Response<User>();
			var user = hc.Users.FindAsync(id).Result;
			if (user is User)
			{
				user.Email = res.Email;
				user.Name = res.Name;
				user.UpdatedTime = DateTime.UtcNow;
				user.Baned = res.Baned;
				user.Phone = res.Phone;
				if (res.Password != "")
				{
					user.Password = res.Password;
				}
				user.Role = res.Role;
				user.ChangePassword = res.ChangePassword;
				user.UpdatedBy = 0;

				if (hc.SaveChangesAsync().Result != 0)
				{
					user.Password = "";
					result.Success = user;
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

		public Response<List<User>> GetAll(HotelContext hc)
		{
			Response<List<User>> result = new Response<List<User>>();
			var users = hc.Users.ToListAsync().Result;
			if (users is List<User>)
			{
				users.ForEach((user) => user.Password = "");
				result.Success = users;
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
