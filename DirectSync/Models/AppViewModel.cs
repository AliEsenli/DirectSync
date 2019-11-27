using System.Collections.Generic;
using System.Linq;


namespace DirectSync.Models
{
    public class AppViewModel
    {
        private readonly ApplicationUser _user;

        public AppViewModel(ApplicationUser user, Asset btcPrice)
        {
            this._user = user;

            this.UserAssets = _user.UserAssets.Select(t =>
               new UserAsset { AssetId = t.AssetId, Amount = t.Amount, Asset = t.Asset })
                .OrderByDescending(a => a.Amount)
                .ToList();

            this.BaseCurrencyPrice = btcPrice.CurrentPrice;
        }

        public string Vorname => _user.Id;
        public string Nachname => _user.UserName;
        public decimal BaseCurrencyPrice { get; }
        public ICollection<UserAsset> UserAssets { get; }
    }
}
