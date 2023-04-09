﻿using HotelAPI2.Common;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HotelAPI2.Domain
{
    public class Reservation : AuditableBaseEntity
    {
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual ICollection<User> Users { get; set; }
        public int RoomId { get; set; }
        [ForeignKey(nameof(RoomId))]
        public virtual Room Room { get; set; }
        public virtual ICollection<Client> Clients { get; set; }
        
    }
}
