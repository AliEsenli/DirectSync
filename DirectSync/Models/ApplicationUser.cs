using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DirectSync.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<UserAsset> UserAssets { get; set; }
        public ICollection<UserKey> UserKeys { get; set; }
    }
}
