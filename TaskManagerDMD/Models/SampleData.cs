using System;
using System.Linq;
using TaskManagerDMD.Models;

namespace TaskManagerDMD
{
    public static class SampleData
    {
        public static void Initialize(TaskContext context)
        {
            if (!context.Tasks.Any())
            {
                context.Tasks.AddRange(                    
                    new TmTask
                    {
                        //Id =1,
                        ParentId = 0,
                        Task_Name = "Задача 1",
                        Task_Description = "Описание задачи 1",
                        Executive_List = "Список исполнителей 1",
                        Registration_Date = new DateTime(2019, 12, 12, 6, 0, 0),
                        Task_Status = "Выполнено",
                        Planned_Duration = new Int64(),
                        Actual_Duration = new Int64(),
                        Completion_Date = new DateTime(2019, 12, 12, 6, 15, 0),
                    },
                    new TmTask
                    {
                        //Id=2,
                        ParentId = 0,
                        Task_Name = "Задача 2",
                        Task_Description = "Описание задачи 2",
                        Executive_List = "Список исполнителей 2",
                        Registration_Date = new DateTime(2019, 12, 12, 6, 0, 0),
                        Task_Status = "Выполнено",
                        Planned_Duration = new Int64(),
                        Actual_Duration = new Int64(),
                        Completion_Date = new DateTime(2019, 12, 12, 6, 15, 0),
                    },
                    new TmTask
                    {
                        //Id=3,
                        ParentId = 0,
                        Task_Name = "Задача 3",
                        Task_Description = "Описание задачи 3",
                        Executive_List = "Список исполнителей 3",
                        Registration_Date = new DateTime(2019, 12, 12, 6, 0, 0),
                        Task_Status = "Выполнено",
                        Planned_Duration = new Int64(),
                        Actual_Duration = new Int64(),
                        Completion_Date = new DateTime(2019, 12, 12, 6, 15, 0),
                    },
                    new TmTask
                    {
                        //Id = 4,
                        ParentId = 1,
                        Task_Name = "Задача 4",
                        Task_Description = "Описание задачи 4",
                        Executive_List = "Список исполнителей 4",
                        Registration_Date = new DateTime(2019, 12, 12, 6, 0, 0),
                        Task_Status = "Выполнено",
                        Planned_Duration = new Int64(),
                        Actual_Duration = new Int64(),
                        Completion_Date = new DateTime(2019, 12, 12, 6, 15, 0),
                    },
                    new TmTask
                    {
                        //Id = 4,
                        ParentId = 4,
                        Task_Name = "Задача 5",
                        Task_Description = "Описание задачи 5",
                        Executive_List = "Список исполнителей 5",
                        Registration_Date = new DateTime(2019, 12, 12, 6, 0, 0),
                        Task_Status = "Выполнено",
                        Planned_Duration = new Int64(),
                        Actual_Duration = new Int64(),
                        Completion_Date = new DateTime(2019, 12, 12, 6, 15, 0),
                    }
                );
                context.SaveChanges();
            }
        }
    }
}