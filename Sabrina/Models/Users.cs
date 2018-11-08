using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class Users
    {
        public Users()
        {
            Messages = new HashSet<Messages>();
            Slavereports = new HashSet<Slavereports>();
        }

        public long UserId { get; set; }
        public DateTime? BanTime { get; set; }
        public string BanReason { get; set; }
        public DateTime? MuteTime { get; set; }
        public string MuteReason { get; set; }
        public DateTime? LockTime { get; set; }
        public string LockReason { get; set; }
        public DateTime? DenialTime { get; set; }
        public DateTime? RuinTime { get; set; }
        public DateTime? SpecialTime { get; set; }
        public string SpecialReason { get; set; }
        public int? WalletEdges { get; set; }
        public int? TotalEdges { get; set; }

        public KinkHashes KinkHashes { get; set; }
        public UserSettings UserSettings { get; set; }
        public ICollection<Messages> Messages { get; set; }
        public ICollection<Slavereports> Slavereports { get; set; }
    }
}
