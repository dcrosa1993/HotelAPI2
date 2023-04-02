using HotelAPI2.Domain;
using System.Diagnostics.CodeAnalysis;

namespace HotelAPI2.Common
{
	public class AuditableBaseEntity
	{
		public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
		[AllowNull]
		public DateTime UpdatedTime { get; set;}
		[AllowNull]
		public User CreatedBy { get; set;}
		[AllowNull]
		public User UpdatedBy { get; set;}
	}
}
