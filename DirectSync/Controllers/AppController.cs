using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DirectSync.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DirectSync.Data;

namespace DirectSync.Controllers
{
    public class AppController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            this._context = context;
            this._userManager = userManager;
        }


        [Authorize]
        public async Task<IActionResult> Index()
        {
            // Get User Object using UserManager
            var user = await _userManager.FindByEmailAsync(User.Identity.Name);

            var realUser = await _context.Users
                .Include(u => u.UserAssets)
                    .ThenInclude(a => a.Asset)
                .Include(u => u.UserKeys)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            //var _private_api = new CCXT.NET.Binance.Private.PrivateApi("YzV1BUo6J0Yib7pRjSElprPM1YOWDvZsOadtXcAgRNzTz5P6u2maKJKQGIZGbkz8", "L4zTKrpBv2EY9zWOG7ZPGcVkMymsvpT7Gp1Tzs7PyI5Z54z1vBWeuuX8pqNxl8ZL");

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

                    CCXT.NET.Binance.Private.PrivateApi _private_api = new CCXT.NET.Binance.Private.PrivateApi("", "");
                    //CCXT.NET.Bittrex.Private.PrivateApi _private_api2 = new CCXT.NET.Bittrex.Private.PrivateApi("", "");
                    switch (exchange.ExchangeId)
                    {
                        // 1. Binance API Call
                        case 1:
                            _private_api = new CCXT.NET.Binance.Private.PrivateApi(publicKey, privateKey);
                            break;
                        // Integrate More Exchanges Here... For example Bittrex  
                        case 2:
                            var _private_api2 = new CCXT.NET.Bittrex.Private.PrivateApi(publicKey, privateKey);
                            break;
                    }
                    // Fetch User Balances using CCXT.NET Library
                    var _balances = await _private_api.FetchBalances();

                    // Create some structure for filtering User Assets
                    var newUserAssets = new List<UserAsset>();
                    var removeUserAssets = new List<UserAsset>();
                    var currentUserAssets = new List<UserAsset>();
                    var userHasAsset = new UserAsset();

                    foreach (var balance in _balances.result)
                    {
                        // Filter Current User Assets
                        var filtered = realUser.UserAssets.Where(i => balance.currency.Contains(i.Asset.AssetShortName));
                        currentUserAssets.AddRange(filtered);

                        // Check if Asset is being supported in Applications Database 
                        var asset = await _context.Assets.FirstOrDefaultAsync(a => a.AssetShortName == balance.currency);
                        if (asset != null)
                        {
                            userHasAsset = realUser.UserAssets.FirstOrDefault(ua => ua.AssetId == asset.AssetId);
                        }

                        // If Asset is supported create new UserAsset Object
                        if (userHasAsset == null && asset != null)
                        {
                            var userAsset = new UserAsset { 
                                UserId = user.Id,
                                ExchangeId = exchange.ExchangeId,
                                AssetId = asset.AssetId,
                                Amount = (double)balance.free
                                };

                            // Add to New User Assets List
                            newUserAssets.Add(userAsset);
                        }
                        else if(userHasAsset != null)
                        {
                            // Just update the Amount
                            userHasAsset.Amount = (double)balance.total;
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
            
            var viewModel = new AppViewModel(realUser);
            return View(viewModel);
        }

        [Authorize]
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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ConnectionDel(ConnectionViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(User.Identity.Name);

            var exchangeId = model.UserKey.ExchangeId;
            var realUser = await _context.Users
                .Include(u => u.UserKeys)
                .Include(u => u.UserAssets)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            var deleteApiKey = realUser.UserKeys.FirstOrDefault(uk => uk.ExchangeId == model.UserKey.ExchangeId);
            var deleteAssets = realUser.UserAssets.Where(ua => ua.ExchangeId == model.UserKey.ExchangeId);

            _context.RemoveRange(deleteAssets);
            _context.Remove(deleteApiKey);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Connection));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Connection(ConnectionViewModel model)
        {
            model.UserKey.User = await _userManager.FindByEmailAsync(User.Identity.Name);
            var newKey = new UserKey {
                ExchangeId = model.UserKey.ExchangeId,
                PublicKey = model.UserKey.PublicKey,
                PrivateKey = model.UserKey.PrivateKey,
                UserId = model.UserKey.User.Id,
                User = model.UserKey.User
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
