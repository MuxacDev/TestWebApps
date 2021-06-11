using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaskManagerDMD.Models;
using Newtonsoft.Json;
using System.Windows;


namespace TaskManagerDMD.Controllers
{
    public class HomeController : Controller
    {

        TaskContext db;
        public HomeController(TaskContext context)
        {
            db = context;
        }
        public IActionResult Index()
        {
            //return View(db.Tasks.ToList());

            List<TreeViewNode> nodes = new List<TreeViewNode>();

            

            //Loop and add the Child Nodes.
            foreach (TmTask task in db.Tasks)
            {
                if (task.ParentId == 0)
                {
                    //nodes.Add(new TreeViewNode { id = "#-" + task.Id.ToString(), parent = "#", text = task.Task_Name });
                    nodes.Add(new TreeViewNode { id = task.Id.ToString(), parent = "#", text = task.Task_Name });
                }
                else
                {
                    //nodes.Add(new TreeViewNode { id = task.ParentId.ToString()+"-" + task.Id.ToString(), parent = task.ParentId.ToString(), text = task.Task_Name });
                    nodes.Add(new TreeViewNode { id = task.Id.ToString(), parent = task.ParentId.ToString(), text = task.Task_Name });
                }

                

            }

            //Serialize to JSON string.
            ViewBag.Json = JsonConvert.SerializeObject(nodes);


            return View(db.Tasks.ToList());
        }

        [HttpPost]
        public IActionResult Index(string selectedItems)
        {
            List<TreeViewNode> items = JsonConvert.DeserializeObject<List<TreeViewNode>>(selectedItems);
            return RedirectToAction("Index");
        }


        /*[HttpPost]
        public JsonResult SubtasksPartialView(string id)
        {
            List<TmTask> tasks = new List<TmTask>();
            foreach (TmTask task in db.Tasks)
            {
                if (task.ParentId.ToString() == id)
                {
                    tasks.Add(task);
                }
            }
            return Json(tasks);
        }*/
        [HttpGet]
        public ActionResult GetSubtasks(string id)
        {
            List<TmTask> tasks = new List<TmTask>();
            foreach (TmTask task in db.Tasks)
            {
                if (task.ParentId.ToString() == id)
                {
                    tasks.Add(task);
                }
            }             
            return PartialView("_SubtasksPartialView",tasks);
        }


    }
}
