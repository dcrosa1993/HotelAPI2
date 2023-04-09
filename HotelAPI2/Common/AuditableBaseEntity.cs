using HotelAPI2.Domain;
using System.Diagnostics.CodeAnalysis;

namespace HotelAPI2.Common
{
	public class AuditableBaseEntity
	{
		public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
		[AllowNull]
		public DateTime UpdatedTime { get; set;}
		public int CreatedBy { get; set;}
		public int UpdatedBy { get; set;}
	}
}
