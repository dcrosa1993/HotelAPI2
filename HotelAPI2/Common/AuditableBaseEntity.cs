

namespace HotelAPI2.Common
{
	public class AuditableBaseEntity
	{
		public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedTime { get; set;}
		public string CreatedBy { get; set;} = string.Empty;
		public string UpdatedBy { get; set; } = string.Empty;
	}
}
