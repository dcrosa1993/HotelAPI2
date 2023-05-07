
using System.Diagnostics.CodeAnalysis;

namespace HotelAPI2.DTOs
{
	public class ClientInput
	{
		public string Name { get; set; }
		public string Passport { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public string Age { get; set; } = "adult";

	}
}
