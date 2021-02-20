using leave_management.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Contracts
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<LeaveType> leaveTypes { get; }
        IGenericRepository<LeaveAllocation> leaveAllocations { get; }
        IGenericRepository<LeaveRequest> leaveRequests { get; }
        Task Save();
    }
}
