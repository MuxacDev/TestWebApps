using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagerDMD.Models
{
    public class TmTask
    {
        public Int32 Id { get; set; }
        public Int32 ParentId { get; set; }
        public String Task_Name { get; set; }
        public String Task_Description { get; set; }
        public String Executive_List { get; set; }
        public DateTime Registration_Date { get; set; }
        public String Task_Status { get; set; }
        public Int64 Planned_Duration { get; set; }
        public Int64 Actual_Duration { get; set; }
        public DateTime Completion_Date { get; set; }
    }
}
