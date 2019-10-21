using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DirectSync.Models
{
    public class Asset
    {
        [Key]
        public int AssetId { get; set; }
        public string AssetName { get; set; }
        public string AssetShortName { get; set; }
        public decimal CurrentPrice { get; set; }
    }
}
