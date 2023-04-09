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
		public string Name { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public string Phone { get; set; }
		public string Role { get; set; }
		public bool Baned { get; set; } = false;
		public virtual ICollection<UserAccess> UserAccess { get; set; }
		public bool ChangePassword { get; set; } = false;
		public virtual ICollection<Reservation> Reservations { get; set; }
	}
}
