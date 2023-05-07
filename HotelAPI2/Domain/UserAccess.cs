using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using HotelAPI2.Common;

namespace HotelAPI2.Domain
{
	public class UserAccess:AuditableBaseEntity
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public string Description { get; set; } = string.Empty;
		public ICollection<User>? Users { get; set; }
	}
}
