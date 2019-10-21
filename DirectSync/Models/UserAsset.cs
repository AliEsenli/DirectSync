using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Threading.Tasks;

namespace DirectSync.Models
{
    public class UserAsset 
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int ExchangeId { get; set; }
        public int AssetId { get; set; }
        public Asset Asset { get; set; }
        public double Amount { get; set; }
    }
}
