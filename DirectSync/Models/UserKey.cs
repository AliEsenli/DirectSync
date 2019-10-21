using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DirectSync.Models
{
    public class UserKey
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public Exchange Exchange { get; set; }
        public int ExchangeId { get; set; }

        [Required]
        [DisplayName("Public Key")]
        public string PublicKey { get; set; }

        [Required]
        [DisplayName("Private Key")]
        public string PrivateKey { get; set; }
    }
}
