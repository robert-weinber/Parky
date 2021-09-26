using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkyWeb.Models;
using ParkyWeb.Models.ViewModel;
using ParkyWeb.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ParkyWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly INationalParkRepository _npRepo;
        private readonly ITrailRepository _tRepo;
        private readonly IAccountRepository _accRepo;

        public HomeController(ILogger<HomeController> logger, INationalParkRepository npRepo, ITrailRepository tRepo, IAccountRepository accRepo)
        {
            _logger = logger;
            _npRepo = npRepo;
            _tRepo = tRepo;
            _accRepo = accRepo;
        }

        public async Task<IActionResult> Index()
        {
            IndexVM listOfParksAndTrails = new IndexVM()
            {
                NationalParkList = await _npRepo.GetAllAsync(SD.NationalParkAPIPath, HttpContext.Session.GetString("JWToken")),
                TrailList = await _tRepo.GetAllAsync(SD.TrailAPIPath, HttpContext.Session.GetString("JWToken"))
            };
            return View(listOfParksAndTrails);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpGet]
        public IActionResult Login()
        {
            User obj = new User();
            return View(obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(User obj)
        {
            User objUser = await _accRepo.LoginAsync(SD.UserAPIPath + "authenticate/", obj);
            if (objUser == null)
            {
                return View();
            }
            HttpContext.Session.SetString("JWToken", objUser.Token);
            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User obj)
        {
            bool result = await _accRepo.RegisterAsync(SD.UserAPIPath + "register/", obj);
            if (!result)
            {
                return View();
            }
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.SetString("JWToken", "");
            return RedirectToAction("Index");
        }
    }
}
