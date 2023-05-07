using HotelAPI2.Common;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HotelAPI2.Domain
{
	public class User:AuditableBaseEntity
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public string Phone { get; set; } = string.Empty;
		public string Role { get; set; } = string.Empty;
		public bool Baned { get; set; } = false;
		public ICollection<UserAccess> UserAccess { get; set; } = new List<UserAccess>();
		public bool ChangePassword { get; set; } = false;
		public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
	}
}
