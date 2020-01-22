using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DirectSync.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using DirectSync.Data;
using Binance.Net;
using CryptoExchange.Net.Authentication;
using Binance.Net.Objects;
using Microsoft.EntityFrameworkCore;

namespace DirectSync.Controllers
{
    [Authorize]
    public class AppController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            this._context = context;
            this._userManager = userManager;
        }


        public async Task<IActionResult> Index()
        {
            // Get User Object using UserManager
            var user = await _userManager.FindByEmailAsync(User.Identity.Name);

            var realUser = await _context.Users
                .Include(u => u.UserAssets)
                    .ThenInclude(a => a.Asset)
                .Include(u => u.UserKeys)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            // Get Users All Exchange Connections
            var userExchangeConnectionList = realUser.UserKeys.ToList();

            // Iterate through each Connection
            foreach (var exchange in userExchangeConnectionList)
            {
                // Get the correct Exchange Market by comparing ExchangeID's
                var exchangeConnection = realUser.UserKeys.FirstOrDefault(uk => uk.ExchangeId == exchange.ExchangeId);

                if (exchangeConnection != null)
                {
                    // Get Users public API KEY
                    var publicKey = exchangeConnection.PublicKey;
                    // Get Users private API KEY
                    var privateKey = exchangeConnection.PrivateKey;

                    switch (exchange.ExchangeId)
                    {
                        // 1. Binance API Call
                        case 1:
                            //_private_api = new CCXT.NET.Binance.Private.PrivateApi(publicKey, privateKey);
                            BinanceClient.SetDefaultOptions(new BinanceClientOptions
                            {
                                ApiCredentials = new ApiCredentials(publicKey, privateKey)
                            });
                            break;
                        // Integrate More Exchanges Here... For example Bittrex  
                        case 2:
                            //
                            break;
                    }


                    BinanceClient binanceClient = new BinanceClient();

                    // Fetch User Balances using CryptoExchange.NET Library
                    var info = await binanceClient.GetAccountInfoAsync();

                    binanceClient.Dispose();

                    // Create some structure for filtering User Assets
                    var newUserAssets = new List<UserAsset>();
                    var removeUserAssets = new List<UserAsset>();
                    var currentUserAssets = new List<UserAsset>();
                    var userHasAsset = new UserAsset();


                    foreach (var balance in info.Data.Balances.Where(b => (double)b.Total > 0.0001))
                    {
                        // Filter Current User Assets
                        var filtered = realUser.UserAssets.Where(i => balance.Asset.Contains(i.Asset.AssetShortName));
                        var filtered3 = realUser.UserAssets.Where(ua => balance.Asset == ua.Asset.AssetShortName);

                        currentUserAssets.AddRange(filtered3);

                        // Check if Asset is being supported in Applications Database 
                        var asset = await _context.Assets.FirstOrDefaultAsync(a => a.AssetShortName == balance.Asset);
                        if (asset != null)
                        {
                            userHasAsset = realUser.UserAssets.FirstOrDefault(ua => ua.AssetId == asset.AssetId);
                        }

                        // If Asset is supported create new UserAsset Object
                        if (userHasAsset == null && asset != null)
                        {
                            var userAsset = new UserAsset
                            {
                                UserId = user.Id,
                                ExchangeId = exchange.ExchangeId,
                                AssetId = asset.AssetId,
                                Amount = (double)balance.Free
                            };

                            // Add to New User Assets List
                            newUserAssets.Add(userAsset);
                        }
                        else if (userHasAsset != null)
                        {
                            // Just update the Amount
                            userHasAsset.Amount = (double)balance.Total;
                            await _context.SaveChangesAsync();
                        }
                    }

                    // Preparing the Assets which should be deleted by comparing two Lists
                    var deleteAssets = realUser.UserAssets.ToList().Except(currentUserAssets).ToList();
                    removeUserAssets.AddRange(deleteAssets);

                    // Save the new added Assets to Database
                    await _context.AddRangeAsync(newUserAssets);
                    // Delete Assets which aren't in Users Portfolio anymore.
                    _context.RemoveRange(removeUserAssets);
                    // Save All Changes
                    await _context.SaveChangesAsync();

                }
            }

            var btcPrice = await _context.Assets.FirstOrDefaultAsync(a => a.AssetShortName == "BTC");

            var viewModel = new AppViewModel(realUser, btcPrice);
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Connection()
        {
            var user = await _userManager.FindByEmailAsync(User.Identity.Name);

            var realUser = await _context.Users
                .Include(u => u.UserKeys)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            var viewModel = new ConnectionViewModel(realUser, _context);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConnectionDel([FromForm]UserKey userKey)
        {
            var currentUser = await _userManager.FindByEmailAsync(User.Identity.Name);
            var exchangeId = userKey.ExchangeId;

            var realUser = await _context.Users
                .Include(u => u.UserKeys)
                .Include(u => u.UserAssets)
                .FirstOrDefaultAsync(u => u.Id == currentUser.Id);

            var deleteApiKey = realUser.UserKeys.FirstOrDefault(uk => uk.ExchangeId == exchangeId);
            var deleteAssets = realUser.UserAssets.Where(ua => ua.ExchangeId == exchangeId);

            _context.RemoveRange(deleteAssets);
            _context.Remove(deleteApiKey);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Connection));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Connection([FromForm]UserKey userKey)
        {
            var currentUser = await _userManager.FindByEmailAsync(User.Identity.Name);

            var newKey = new UserKey
            {
                ExchangeId = userKey.ExchangeId,
                PublicKey = userKey.PublicKey,
                PrivateKey = userKey.PrivateKey,
                UserId = currentUser.Id,
                User = currentUser
            };

            await _context.AddAsync(newKey);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Connection));
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
