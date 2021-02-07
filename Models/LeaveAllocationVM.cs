using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Models
{
    public class LeaveAllocationVM
    {
        public int Id { get; set; }

        public int NumberOfDays { get; set; }

        public int Period { get; set; }

        public DateTime DateCreated { get; set; }


        public EmployeeVM Employee { get; set; }
        public string EmployeeId { get; set; }


        public LeaveTypeVM LeaveType { get; set; }
        public int LeaveTypeId { get; set; }

        public IEnumerable<SelectListItem> Employees { get; set; }
        public IEnumerable<SelectListItem> LeaveTypes { get; set; }
    }

    public class CreateLeaveAllocationVM
    {
        public List<LeaveTypeVM> LeaveTypes { get; set; }
        public int NumberUpdated { get; set; }
    }

    public class EditLeaveAllocationVM
    {
        public int Id { get; set; }

        [Display(Name ="Number Of Days")]
        public int NumberOfDays { get; set; }

        public EmployeeVM Employee { get; set; }
        public string EmployeeId { get; set; }

        public LeaveTypeVM LeaveType { get; set; }
    }
    
    public class ViewsAllocationVM
    {
        public EmployeeVM Employee { get; set; }
        public int EmployeeId { get; set; }
        public List<LeaveAllocationVM> LeaveAllocations { get; set; }
    }

}
