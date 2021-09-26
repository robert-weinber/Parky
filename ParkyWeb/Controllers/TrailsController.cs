using Microsoft.AspNetCore.Mvc;
using ParkyWeb.Models;
using ParkyWeb.Models.ViewModel;
using ParkyWeb.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace ParkyWeb.Controllers
{
    [Authorize]
    public class TrailsController : Controller
    {
        private readonly ITrailRepository _tRepo;
        private readonly INationalParkRepository _npRepo;
        public TrailsController(ITrailRepository tRepo, INationalParkRepository npRepo)
        {
            _tRepo = tRepo;
            _npRepo = npRepo;
        }
        public IActionResult Index()
        {
            return View(new Trail() { });
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Upsert(int? id)
        {
            IEnumerable<NationalPark> npList = await _npRepo.GetAllAsync(SD.NationalParkAPIPath, HttpContext.Session.GetString("JWToken"));
            TrailsVM objVM = new TrailsVM()
            {
                NationalParkList = npList.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }
                ),
                Trail = new Trail()
            };

            if(id == null)
            {// create
                return View(objVM);
            }
            //update
            objVM.Trail = await _tRepo.GetAsync(SD.TrailAPIPath, id.GetValueOrDefault(), HttpContext.Session.GetString("JWToken"));
            if(objVM.Trail == null)
            {
                return NotFound();
            }
            return View(objVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(TrailsVM obj)
        {
            if (ModelState.IsValid)
            {
                if(obj.Trail.Id==0)
                {
                    await _tRepo.CreateAsync(SD.TrailAPIPath, obj.Trail, HttpContext.Session.GetString("JWToken"));
                }
                else
                {
                    await _tRepo.UpdateAsync(SD.TrailAPIPath+obj.Trail.Id, obj.Trail, HttpContext.Session.GetString("JWToken"));
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                IEnumerable<NationalPark> npList = await _npRepo.GetAllAsync(SD.NationalParkAPIPath, HttpContext.Session.GetString("JWToken"));
                TrailsVM objVM = new TrailsVM()
                {
                    NationalParkList = npList.Select(i => new SelectListItem
                    {
                        Text = i.Name,
                        Value = i.Id.ToString()
                    }
                    ),
                    Trail = obj.Trail
                };
                return View(objVM);
            }
        }

        public async Task<IActionResult> GetAllTrail()
        {
            return Json(new { data = await _tRepo.GetAllAsync(SD.TrailAPIPath, HttpContext.Session.GetString("JWToken")) });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var status = await _tRepo.DeleteAsync(SD.TrailAPIPath, id, HttpContext.Session.GetString("JWToken"));
            if (status)
            {
                return Json(new { success = true, message="Delete Successful" });
            }
            return Json(new { success = false, message = "Delete Not Successful" });
        }
    }
}
