using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MobileStore.Models;

namespace MobileStore.Controllers
{
    public class HomeController : Controller
    {
        //конструктор в котором получаем контекст данных
        MobileContext db;
        public HomeController(MobileContext context)
        {
            db = context;
        }
        public IActionResult Index()
        {
            //Каждый метод контроллера по умоланию использует одноименное представление (в соответствии с соглашением) - здесь Index
            List<Phone> list = db.Phones.ToList();
            return View(list);
        }

        [HttpGet]
        public IActionResult Buy(int? id)
        {
            if (id == null) return RedirectToAction("Index");
            ViewBag.PhoneId = id;
            return View();
        }
        [HttpPost]
        public string Buy(Order order)
        {
            db.Orders.Add(order);
            // сохраняем в бд все изменения
            db.SaveChanges();
            return "Спасибо, " + order.User + ", за покупку!";
        }

        /*[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }*/
    }
}
