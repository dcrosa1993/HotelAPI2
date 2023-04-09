
using System.Diagnostics.CodeAnalysis;

namespace HotelAPI2.DTOs
{
	public class UserInput
	{
		public string Name { get; set; }
		public string Email { get; set; }
		public string Password { get; set; } = String.Empty;
		public string Phone { get; set; }
		public string Role { get; set; }
		public bool Baned { get; set; }
		public bool ChangePassword { get; set; }

	}
}
