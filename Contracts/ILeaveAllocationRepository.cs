using leave_management.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Contracts
{
    public interface ILeaveAllocationRepository : IRepositoryBase<LeaveAllocation>
    {
        Task<bool> CheckAllocation(int leaveId, string employeeId);
        Task<ICollection<LeaveAllocation>> GetLeaveAllocationByEmployee(string employeeId);
        Task<LeaveAllocation> GetLeaveAllocationByEmployeeAndLeaveType(string employeeId, int leaveTypeId);
    }
}
