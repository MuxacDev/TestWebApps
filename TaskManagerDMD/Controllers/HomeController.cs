using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TaskManagerDMD.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Localization;
using System.IO;
using System.Diagnostics;


namespace TaskManagerDMD.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStringLocalizer<HomeController> _localizer;
        /*public HomeController(IStringLocalizer<HomeController> localizer)
        {
            _localizer = localizer;
        }*/
        static string m_culture="ru";
        static int m_selectedId; //статическое поле для хранения Id выбранного нода         
        static TmTask m_taskToEdit; //статическое поле для хранения объекта редактируемой задачи

        TaskContext db; //поле контекста БД

        public HomeController(TaskContext context, IStringLocalizer<HomeController> localizer)
        {
            _localizer = localizer;
            db = context;
        }
        public IActionResult Index()
        {
            try
            {
                //активация локализации
                SetViewDataForIndex();


                //пересчет трудозатрат
                List<long[]> paramList = new List<long[]>();
                foreach (TmTask task in db.Tasks)
                {
                    int taskId = task.Id;
                    int parentId = task.ParentId;
                    long plannedDuration = task.PlannedCompletionDate.Ticks - task.RegistrationDate.Ticks;
                    long actualDuration = 0;
                    if (task.TaskStatus == "Завершена" || task.TaskStatus == "Приостановлена")
                    {
                        actualDuration = task.CompletionDate.Ticks - task.RegistrationDate.Ticks;
                    }
                    else actualDuration = DateTime.Now.Ticks - task.RegistrationDate.Ticks;
                    long plannedDurationSubtasks = 0;
                    long actualDurationSubtasks = 0;
                    long plannedDurationSum = plannedDuration;
                    long actualDurationSum = actualDuration;
                    paramList.Add(new long[] { taskId, parentId, plannedDuration, actualDuration, plannedDurationSubtasks, actualDurationSubtasks, plannedDurationSum, actualDurationSum });
                }
                paramList = paramList.OrderByDescending(i => i[1]).ThenByDescending(i => i[0]).ToList();
                for (int i = 0; i < paramList.Count; i++)
                {
                    for (int j = 0; j < paramList.Count; j++)
                    {
                        if (paramList[i][0] == paramList[j][1])
                        {
                            paramList[i][4] += paramList[j][6];
                            paramList[i][5] += paramList[j][7];
                            paramList[i][6] += paramList[j][6];
                            paramList[i][7] += paramList[j][7];
                        }
                    }
                }

                //обновление вычисляемых полей в БД
                foreach (TmTask task in db.Tasks)
                {
                    for (int i = 0; i < paramList.Count; i++)
                    {
                        if (task.Id == paramList[i][0])
                        {
                            task.PlannedDuration = paramList[i][2];
                            task.ActualDuration = paramList[i][3];
                            task.PlannedDurationSubtasks = paramList[i][4];
                            task.ActualDurationSubtasks = paramList[i][5];
                            task.PlannedDurationSum = paramList[i][6];
                            task.ActualDurationSum = paramList[i][7];
                        }
                    }
                    db.Tasks.Update(task);
                }
                //db.SaveChanges();


                //пересчет и поиск детей
                List<string[]> paramList2 = new List<string[]>();
                List<int> idsTemp = new List<int>();
                List<int> parentIdsTemp = new List<int>();
                foreach (TmTask item in db.Tasks)
                {
                    idsTemp.Add(item.Id);
                    parentIdsTemp.Add(item.Id);
                }
                int maxIdLength = idsTemp.Max().ToString().Length;
                int maxParentIdLength = parentIdsTemp.Max().ToString().Length;
                foreach (TmTask item in db.Tasks)
                {
                    int zerosCount = maxIdLength - item.Id.ToString().Length;
                    string prefix = "";
                    for (int i = 0; i < zerosCount; i++)
                    {
                        prefix += "0";
                    }
                    string taskId = prefix + item.Id.ToString();

                    zerosCount = maxParentIdLength - item.ParentId.ToString().Length;
                    prefix = "";
                    for (int i = 0; i < zerosCount; i++)
                    {
                        prefix += "0";
                    }
                    string parentId = prefix + item.ParentId.ToString();

                    string childrenIds = "";
                    paramList2.Add(new string[] { taskId, parentId, childrenIds });
                }
                paramList2 = paramList2.OrderByDescending(i => i[1]).ThenBy(i => i[0]).ToList();
                for (int i = 0; i < paramList2.Count; i++)
                {
                    for (int j = 0; j < paramList2.Count; j++)
                    {
                        if (paramList2[i][0] == paramList2[j][1])
                        {
                            if (paramList2[i][2] == "")
                            {
                                paramList2[i][2] = paramList2[j][0].TrimStart('0');
                            }
                            else
                            {
                                paramList2[i][2] = paramList2[i][2].TrimStart('0') + ";" + paramList2[j][0].TrimStart('0');
                            }
                        }
                    }
                }
                for (int i = 0; i < paramList2.Count; i++)
                {
                    for (int j = 0; j < paramList2.Count; j++)
                    {
                        if (paramList2[i][2] != "" && paramList2[j][2] != "")
                        {
                            if (paramList2[i][0] == paramList2[j][1])
                            {
                                paramList2[i][2] = paramList2[i][2] + ";" + paramList2[j][2];
                            }
                        }
                    }
                }



                //обновление поля с детьми
                foreach (TmTask item in db.Tasks)
                {
                    for (int i = 0; i < paramList2.Count; i++)
                    {
                        if (item.Id.ToString() == paramList2[i][0].TrimStart('0'))
                        {
                            item.ChildrenIds = paramList2[i][2];
                            db.Tasks.Update(item);
                        }
                    }
                }
                db.SaveChanges();


                //обновление дерева задач
                List<TreeViewNode> nodes = new List<TreeViewNode>();
                nodes.Add(new TreeViewNode { id = "0", parent = "#", text = ViewData["AllTasks"].ToString() });
                //Loop and add the Child Nodes.
                foreach (TmTask task in db.Tasks)
                {
                    if (task.ParentId == 0)
                    {
                        nodes.Add(new TreeViewNode { id = task.Id.ToString(), parent = "#", text = task.TaskName });
                    }
                    else
                    {
                        nodes.Add(new TreeViewNode { id = task.Id.ToString(), parent = task.ParentId.ToString(), text = task.TaskName });
                    }
                }
                //Serialize to JSON string.
                ViewBag.Json = JsonConvert.SerializeObject(nodes);                
                return View();
                
            }
            catch (Exception ex)
            {
                FileStream f = new FileStream("Log.txt", FileMode.Append, FileAccess.Write);                
                TextWriterTraceListener writer1 = new TextWriterTraceListener(f);                
                Trace.Listeners.Add(writer1);                
                Trace.WriteLine("Время ошибки: " + DateTime.Now.ToLocalTime().ToString());
                Trace.WriteLine(ex.Message);
                Trace.WriteLine(ex.StackTrace);
                Trace.Flush();
                f.Close();
                return RedirectToAction("Error");
            }
            
        }              
        
        /// <summary>
        /// Получение списка подзадач выбранной задачи
        /// </summary>
        /// <param name="id">Id выбранной задачи</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetSubtasks(string id)
        {
            try
            {
                //активация локализации
                SetViewDataForIndex();

                List<TmTask> tasks = new List<TmTask>();
                foreach (TmTask task in db.Tasks)
                {
                    if (task.ParentId.ToString() == id)
                    {
                        tasks.Add(task);
                    }

                    /*if (id!="0")
                    {
                        for (int i = 0; i < tasks.Count; i++)
                        {
                            if (task.ParentId == tasks[i].Id)
                            {
                                tasks.Add(task);
                            }
                        } 
                    }*/
                }
                return PartialView("SubtasksPartialView", tasks);
            }
            catch (Exception ex)
            {
                FileStream f = new FileStream("Log.txt", FileMode.Append, FileAccess.Write);
                TextWriterTraceListener writer1 = new TextWriterTraceListener(f);
                Trace.Listeners.Add(writer1);
                Trace.WriteLine("Время ошибки: " + DateTime.Now.ToLocalTime().ToString());
                Trace.WriteLine(ex.Message);
                Trace.WriteLine(ex.StackTrace);
                Trace.Flush();
                f.Close();
                return RedirectToAction("Error");
            }

        }

        /// <summary>
        /// Получение детальной информации о выбранной в дереве задаче
        /// </summary>
        /// <param name="id">Id выбранного узла</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetTaskDetails(string id)
        {
            try
            {
                //активация локализации
                SetViewDataForIndex();

                TmTask task = new TmTask();
                foreach (TmTask item in db.Tasks)
                {
                    if (item.Id.ToString() == id)
                    {
                        task = item;
                        break;
                    }

                }
                //отключение показа ссылки на удаление задачи
                ViewBag.Children = false;
                foreach (TmTask item in db.Tasks)
                {
                    if (item.ParentId.ToString() == id)
                    {
                        ViewBag.Children = true;
                        break;
                    }
                }


                return PartialView("TaskDetailsPartialView", task);
            }
            catch (Exception ex)
            {
                FileStream f = new FileStream("Log.txt", FileMode.Append, FileAccess.Write);
                TextWriterTraceListener writer1 = new TextWriterTraceListener(f);
                Trace.Listeners.Add(writer1);
                Trace.WriteLine("Время ошибки: " + DateTime.Now.ToLocalTime().ToString());
                Trace.WriteLine(ex.Message);
                Trace.WriteLine(ex.StackTrace);
                Trace.Flush();
                f.Close();
                return RedirectToAction("Error");
            }
        }

        /// <summary>
        /// Добавление на страницу Index.html ссылки на создание подзадачи
        /// </summary>
        /// <param name="id">Id задачи, передаваемый частичному представлению "Создание подзадачи"</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetTaskCreationLink(int id)
        {
            try
            {
                //активация локализации
                SetViewDataForIndex();

                return PartialView("TaskCreationLinkPartialView", id);
            }
            catch (Exception ex)
            {
                FileStream f = new FileStream("Log.txt", FileMode.Append, FileAccess.Write);
                TextWriterTraceListener writer1 = new TextWriterTraceListener(f);
                Trace.Listeners.Add(writer1);
                Trace.WriteLine("Время ошибки: " + DateTime.Now.ToLocalTime().ToString());
                Trace.WriteLine(ex.Message);
                Trace.WriteLine(ex.StackTrace);
                Trace.Flush();
                f.Close();
                return RedirectToAction("Error");
            }
        }

        /// <summary>
        /// Вызов формы ввода полей создаваемой подзадачи
        /// </summary>
        /// <param name="id">Id выбранного узла, передаваемый создаваемой подзадаче как свойство ParentId</param>
        /// <returns></returns>
        public IActionResult Create(int id)
        {
            try
            {
                //активация локализации
                SetViewDataForCreate();

                m_selectedId = id;//переменная со значением ParentId для создаваемой подзадачи

                ViewBag.Tasks = db.Tasks;//переменная со списком задач для формы создания подзадачи
                ViewBag.SelectedId = id;//передается в форму создания подзадачи

                return View();
            }
            catch (Exception ex)
            {
                FileStream f = new FileStream("Log.txt", FileMode.Append, FileAccess.Write);
                TextWriterTraceListener writer1 = new TextWriterTraceListener(f);
                Trace.Listeners.Add(writer1);
                Trace.WriteLine("Время ошибки: " + DateTime.Now.ToLocalTime().ToString());
                Trace.WriteLine(ex.Message);
                Trace.WriteLine(ex.StackTrace);
                Trace.Flush();
                f.Close();
                return RedirectToAction("Error");
            }
        }
       
        /// <summary>
        /// Получение объекта создаваемой в форме подзадачи  и запись в БД
        /// </summary>
        /// <param name="task">объект создаваемой подзадачи</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Create(TmTask task)
        {
            try
            {
                //назначение автозаполняемых полей созданной задачи

                task.ParentId = m_selectedId;
                task.RegistrationDate = DateTime.Now;
                if (task.PlannedCompletionDate < task.RegistrationDate)
                {
                    task.PlannedCompletionDate = DateTime.Now.AddDays(7);
                }
                db.Tasks.Add(task);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                FileStream f = new FileStream("Log.txt", FileMode.Append, FileAccess.Write);
                TextWriterTraceListener writer1 = new TextWriterTraceListener(f);
                Trace.Listeners.Add(writer1);
                Trace.WriteLine("Время ошибки: " + DateTime.Now.ToLocalTime().ToString());
                Trace.WriteLine(ex.Message);
                Trace.WriteLine(ex.StackTrace);
                Trace.Flush();
                f.Close();
                return RedirectToAction("Error");
            }
        }

        /// <summary>
        /// Метод сохранения изменяемого задания редактирования
        /// </summary>
        /// <param name="id">Id выбранного узла</param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            try
            {
                //активация локализации
                SetViewDataForEdit();

                foreach (TmTask task in db.Tasks)
                {
                    if (id == task.Id)
                    {
                        ViewBag.TaskToEdit = task;
                        m_taskToEdit = task;
                        break;
                    }
                }
                return View();
            }
            catch (Exception ex)
            {
                FileStream f = new FileStream("Log.txt", FileMode.Append, FileAccess.Write);
                TextWriterTraceListener writer1 = new TextWriterTraceListener(f);
                Trace.Listeners.Add(writer1);
                Trace.WriteLine("Время ошибки: " + DateTime.Now.ToLocalTime().ToString());
                Trace.WriteLine(ex.Message);
                Trace.WriteLine(ex.StackTrace);
                Trace.Flush();
                f.Close();
                return RedirectToAction("Error");
            }
        }

        /// <summary>
        /// Метод сохранения изменяемого задания редактирования
        /// </summary>
        /// <param name="task">временный объект, заменяющий изменяемую задачу при сохранении</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(TmTask task)
        {
            try
            {
                if (task.RegistrationDate < m_taskToEdit.PlannedCompletionDate&& task.RegistrationDate!= new DateTime(0001, 01, 01, 0, 0, 0))
                {
                    m_taskToEdit.RegistrationDate = task.RegistrationDate;
                }              


                if (task.PlannedCompletionDate > m_taskToEdit.RegistrationDate)
                {
                    m_taskToEdit.PlannedCompletionDate = task.PlannedCompletionDate;
                }

                

                m_taskToEdit.TaskName = task.TaskName;
                m_taskToEdit.TaskDescription = task.TaskDescription;
                db.Tasks.Update(m_taskToEdit);
                db.SaveChanges();

                //Статус «Приостановлена» может быть присвоен только после статуса «Выполняется»
                if (task.TaskStatus == "Приостановлена")
                {
                    if (m_taskToEdit.TaskStatus == "Выполняется")
                    {
                        m_taskToEdit.TaskStatus = task.TaskStatus;
                        m_taskToEdit.CompletionDate = DateTime.Now;
                        db.Tasks.Update(m_taskToEdit);
                    }
                }
                //Статус «Завершена» может быть присвоен только после статуса «Выполняется»
                else if (task.TaskStatus == "Завершена")
                {
                    if (m_taskToEdit.TaskStatus == "Выполняется")
                    {
                        //список id, которые
                        string[] idsToChange = (m_taskToEdit.ChildrenIds + ";" + m_taskToEdit.Id).Split(';');
                        bool changeStatus = true;
                        List<TmTask> tasksToChange = new List<TmTask>();
                        foreach (string id in idsToChange)
                        {
                            foreach (TmTask item in db.Tasks)
                            {
                                if (item.Id.ToString() == id)
                                {
                                    tasksToChange.Add(item);
                                }
                            }
                        }

                        foreach (TmTask item in tasksToChange)
                        {
                            if (item.TaskStatus != "Выполняется")
                            {
                                changeStatus = false;
                            }
                        }

                        if (changeStatus)
                        {
                            DateTime dt = DateTime.Now;
                            foreach (TmTask item in tasksToChange)
                            {
                                item.TaskStatus = "Завершена";
                                item.CompletionDate = dt;
                                db.Tasks.Update(item);
                            }
                        }
                    }
                }
                else
                {
                    m_taskToEdit.TaskStatus = task.TaskStatus;
                    db.Tasks.Update(m_taskToEdit);
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                FileStream f = new FileStream("Log.txt", FileMode.Append, FileAccess.Write);
                TextWriterTraceListener writer1 = new TextWriterTraceListener(f);
                Trace.Listeners.Add(writer1);
                Trace.WriteLine("Время ошибки: " + DateTime.Now.ToLocalTime().ToString());
                Trace.WriteLine(ex.Message);
                Trace.WriteLine(ex.StackTrace);
                Trace.Flush();
                f.Close();
                return RedirectToAction("Error");
            }
        }

        /// <summary>
        /// Метод удаления нода
        /// </summary>
        /// <param name="id">Id выбранного узла</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Delete(int id)
        {
            try
            {
                bool delete = true;
                TmTask taskToDelete = new TmTask();
                foreach (TmTask task in db.Tasks)
                {
                    if (id == task.Id)
                    {

                        foreach (TmTask item in db.Tasks)
                        {
                            if (id == item.ParentId)
                            {
                                //db.Tasks.Remove(item);
                                delete = false;
                                break;
                            }
                        }
                        if (delete)
                        {
                            taskToDelete = task;
                            break;
                        }
                        else RedirectToAction("Index");

                    }
                }
                db.Tasks.Remove(taskToDelete);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                FileStream f = new FileStream("Log.txt", FileMode.Append, FileAccess.Write);
                TextWriterTraceListener writer1 = new TextWriterTraceListener(f);
                Trace.Listeners.Add(writer1);
                Trace.WriteLine("Время ошибки: " + DateTime.Now.ToLocalTime().ToString());
                Trace.WriteLine(ex.Message);
                Trace.WriteLine(ex.StackTrace);
                Trace.Flush();
                f.Close();
                return RedirectToAction("Error");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private void SetViewDataForCreate()
        {
            ViewBag.Culture = m_culture;
            ViewData["Title"] = _localizer["Title_Create"];            
            ViewData["TaskName"] = _localizer["TaskName"];
            ViewData["SubtaskName"] = _localizer["SubtaskName"];
            ViewData["TaskDescription"] = _localizer["TaskDescription"];
            ViewData["ExecutiveList"] = _localizer["ExecutiveList"];
            ViewData["PlannedCompletionDate"] = _localizer["PlannedCompletionDate"];
            ViewData["TaskStatus"] = _localizer["TaskStatus"];
            ViewData["CompletionDate"] = _localizer["CompletionDate"];
            ViewData["Submit"] = _localizer["SubmitTaskCreate"];
            ViewData["Days"] = _localizer["Days"];
            ViewData["Days_Subtasks"] = _localizer["Days_Subtasks"];
            ViewData["App"] = _localizer["Title_Index"];
        }

        private void SetViewDataForEdit()
        {
            ViewBag.Culture = m_culture;
            ViewData["Title"] = _localizer["Title_Edit"];            
            ViewData["TaskName"] = _localizer["TaskName"];
            ViewData["TaskDescription"] = _localizer["TaskDescription"];
            ViewData["RegistrationDate"] = _localizer["RegistrationDate"];
            ViewData["ExecutiveList"] = _localizer["ExecutiveList"];            
            ViewData["PlannedCompletionDate"] = _localizer["PlannedCompletionDate"];            
            ViewData["TaskStatus"] = _localizer["TaskStatus"];
            ViewData["CompletionDate"] = _localizer["CompletionDate"];            
            ViewData["Submit"] = _localizer["SubmitTaskEdit"];
            ViewData["Days"] = _localizer["Days"];
            ViewData["Days_Subtasks"] = _localizer["Days_Subtasks"];
            ViewData["App"] = _localizer["Title_Index"];
        }

        private void SetViewDataForIndex()
        {
            ViewBag.Culture = m_culture;
            
            ViewData["Header_Index"] = _localizer["Header_Index"];
            ViewData["CreateTask_Index"] = _localizer["CreateTask_Index"];
            ViewData["EditTask_Index"] = _localizer["EditTask_Index"];
            ViewData["DeleteTask_Index"] = _localizer["DeleteTask_Index"];
            ViewData["Title"] = _localizer["Title_Index"];
            ViewData["Subtasks_Index"] = _localizer["Subtasks_Index"];
            ViewData["TaskName"] = _localizer["TaskName"];
            ViewData["TaskDescription"] = _localizer["TaskDescription"];
            ViewData["ExecutiveList"] = _localizer["ExecutiveList"];
            ViewData["RegistrationDate"] = _localizer["RegistrationDate"];
            ViewData["PlannedCompletionDate"] = _localizer["PlannedCompletionDate"];
            ViewData["PlannedDuration"] = _localizer["PlannedDuration"];
            ViewData["TaskStatus"] = _localizer["TaskStatus"];
            ViewData["CompletionDate"] = _localizer["CompletionDate"];
            ViewData["ActualDuration"] = _localizer["ActualDuration"];
            ViewData["Days"] = _localizer["Days"];
            ViewData["Days_Subtasks"] = _localizer["Days_Subtasks"];
            ViewData["AllTasks"] = _localizer["AllTasks"];            
            ViewData["NoSubtasks_Index"] = _localizer["NoSubtasks_Index"];
            ViewData["HierarchyNote_Index"] = _localizer["HierarchyNote_Index"];
            ViewData["App"] = _localizer["Title_Index"];
        }

        public IActionResult SetCulture(string cult)
        {
            m_culture = cult;
            ViewBag.Culture = cult;
            return RedirectToAction("Index", new { culture = cult});
        }
    }
}
