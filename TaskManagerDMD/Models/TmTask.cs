using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace TaskManagerDMD.Models
{
    public class TmTask
    {
        
        public Int32 Id { get; set; }        
        public Int32 ParentId { get; set; }
        public String TaskName { get; set; }
        public String TaskDescription { get; set; }
        public String ExecutiveList { get; set; }
        public DateTime RegistrationDate { get; set; }
        public String TaskStatus { get; set; }
        //[NotMapped]
        public Int64 PlannedDuration { get; set; }
        //[NotMapped]
        public Int64 ActualDuration { get; set; }
        public Int64 PlannedDurationSubtasks { get; set; }
        public Int64 ActualDurationSubtasks { get; set; }
        public Int64 PlannedDurationSum { get; set; }
        public Int64 ActualDurationSum { get; set; }
        public DateTime PlannedCompletionDate { get; set; }
        public DateTime CompletionDate { get; set; }
        public string ChildrenIds { get; set; }
    }
}
