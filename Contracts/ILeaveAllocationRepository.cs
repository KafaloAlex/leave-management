using leave_management.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Contracts
{
    public interface ILeaveAllocationRepository : IRepositoryBase<LeaveAllocation>
    {
        bool CheckAllocation(int leaveId, string employeeId);
        ICollection<LeaveAllocation> GetLeaveAllocationByEmployee(string id);
        LeaveAllocation GetLeaveAllocationByEmployeeAndLeaveType(string id, int leaveTypeId);
    }
}
