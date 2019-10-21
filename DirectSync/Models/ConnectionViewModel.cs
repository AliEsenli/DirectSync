using DirectSync.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DirectSync.Models
{
    public class ConnectionViewModel
    {
        private readonly ApplicationUser _user;
        private readonly ApplicationDbContext _context;

        public ConnectionViewModel(ApplicationUser user, ApplicationDbContext context)
        {
            this._user = user;
            this._context = context;
            this.Exchanges = _context.Exchanges.ToList();
            this.User = _user;
        }

        // Default Constructor
        public ConnectionViewModel()
        {

        }

        public UserKey UserKey { get; set; }
        public ICollection<Exchange> Exchanges { get; }
        public ApplicationUser User { get; }

    }
}
