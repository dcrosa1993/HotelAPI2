
using System.Diagnostics.CodeAnalysis;

namespace HotelAPI2.DTOs
{
	public class UserInput
	{
		public string Name { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public string Phone { get; set; } = string.Empty;
		public string Role { get; set; } = string.Empty;
		public bool Baned { get; set; }
		public bool ChangePassword { get; set; }

	}
}
