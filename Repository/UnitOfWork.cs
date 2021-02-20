using leave_management.Contracts;
using leave_management.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _ctx;
        private IGenericRepository<LeaveType> _leaveTypes;
        private IGenericRepository<LeaveRequest> _leaveRequests;
        private IGenericRepository<LeaveAllocation> _leaveAllocations;

        public UnitOfWork(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public IGenericRepository<LeaveType> leaveTypes
           // => _leaveTypes == null ? leaveTypes : new GenericRepository<LeaveType>(_ctx);
           => _leaveTypes ??= new GenericRepository<LeaveType>(_ctx);

        public IGenericRepository<LeaveAllocation> leaveAllocations
            => _leaveAllocations ??= new GenericRepository<LeaveAllocation>(_ctx);

        public IGenericRepository<LeaveRequest> leaveRequests
            => _leaveRequests ??= new GenericRepository<LeaveRequest>(_ctx);


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool v)
        {
            if (v)
            {
                _ctx.Dispose();
            }
        }

        public async Task Save()
        {
            await _ctx.SaveChangesAsync();
        }
    }
}
