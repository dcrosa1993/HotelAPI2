using HotelAPI2.Common;

namespace HotelAPI2.Domain
{
    public class Reservation : AuditableBaseEntity
    {
        public int Id { get; set; }
        public int NoClients { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime DateIn { get; set; }
        public DateTime DateOut { get; set; }
        public double TotalCost { get; set; }
        public double Discount { get; set; }
        public int TotalNights { get; set; }
        public int PaymentNights { get; set; }
        public double Management { get; set; }
        public double Transport { get; set; }
        public bool AdvanceManagement { get; set; } = false;
        public int RoomId { get; set; }
        public virtual Room Room { get; set; }
        public virtual Client[] Clients { get; set; }
    }
}
