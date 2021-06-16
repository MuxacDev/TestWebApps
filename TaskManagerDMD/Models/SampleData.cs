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
                        ParentId = 0,
                        TaskName = "Задача 1",
                        TaskDescription = "Описание задачи 1",
                        ExecutiveList = "Список исполнителей 1",
                        RegistrationDate = new DateTime(2021, 06, 01, 6, 0, 0),
                        TaskStatus = "Выполняется",                        
                        PlannedCompletionDate = new DateTime(2021, 07, 31, 6, 15, 0),
                    },
                    new TmTask
                    {                        
                        ParentId = 0,
                        TaskName = "Задача 2",
                        TaskDescription = "Описание задачи 2",
                        ExecutiveList = "Список исполнителей 2",
                        RegistrationDate = new DateTime(2021, 06, 01, 6, 0, 0),
                        TaskStatus = "Выполняется",                        
                        PlannedCompletionDate = new DateTime(2021, 07, 31, 6, 15, 0),
                    },
                    new TmTask
                    {                        
                        ParentId = 0,
                        TaskName = "Задача 3",
                        TaskDescription = "Описание задачи 3",
                        ExecutiveList = "Список исполнителей 3",
                        RegistrationDate = new DateTime(2021, 06, 01, 6, 0, 0),
                        TaskStatus = "Выполняется",
                        PlannedCompletionDate = new DateTime(2021, 07, 31, 6, 15, 0),
                    },
                    new TmTask
                    {                        
                        ParentId = 1,
                        TaskName = "Задача 4",
                        TaskDescription = "Описание задачи 4",
                        ExecutiveList = "Список исполнителей 4",
                        RegistrationDate = new DateTime(2021, 06, 01, 6, 0, 0),
                        TaskStatus = "Выполняется",
                        PlannedCompletionDate = new DateTime(2021, 07, 31, 6, 15, 0),
                    },
                    new TmTask
                    {                        
                        ParentId = 4,
                        TaskName = "Задача 5",
                        TaskDescription = "Описание задачи 5",
                        ExecutiveList = "Список исполнителей 5",
                        RegistrationDate = new DateTime(2021, 06, 01, 6, 0, 0),
                        TaskStatus = "Завершена",
                        CompletionDate = new DateTime(2021, 06, 12, 6, 15, 0),
                        PlannedCompletionDate = new DateTime(2021, 07, 31, 6, 15, 0),
                    },
                    new TmTask
                    {
                        ParentId = 4,
                        TaskName = "Задача 6",
                        TaskDescription = "Описание задачи 6",
                        ExecutiveList = "Список исполнителей 6",
                        RegistrationDate = new DateTime(2021, 06, 01, 6, 0, 0),
                        TaskStatus = "Завершена",
                        CompletionDate = new DateTime(2021, 06, 12, 6, 15, 0),
                        PlannedCompletionDate = new DateTime(2021, 07, 31, 6, 15, 0),
                    }
                );
                context.SaveChanges();
            }
        }
    }
}