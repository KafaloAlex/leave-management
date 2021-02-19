﻿using leave_management.Contracts;
using leave_management.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leave_management.Repository
{
    public class LeaveAllocationRepository : ILeaveAllocationRepository
    {
        private readonly ApplicationDbContext _db;

        public LeaveAllocationRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> CheckAllocation(int leaveId, string employeeId)
        {
            var period = DateTime.Now.Year;
            var alloc = await FindAll();
            return alloc.Where(q => q.EmployeeId == employeeId && q.LeaveTypeId == leaveId && q.Period == period).Any();
        }

        public async Task<bool> Create(LeaveAllocation entity)
        {
            await _db.LeaveAllocations.AddAsync(entity);
            return await Save();
        }

        public async Task<bool> Delete(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Remove(entity);
            return await Save();
        }

        public async Task<ICollection<LeaveAllocation>> FindAll()
        {
            var leaveAllocations = await  _db.LeaveAllocations.Include(q => q.LeaveType).Include(q => q.Employee).ToListAsync();
            return leaveAllocations;
        }

        public async Task<LeaveAllocation> FindById(int id)
        {
            var leaveAllocation = await _db.LeaveAllocations.Include(q => q.LeaveType).Include(q => q.Employee).FirstOrDefaultAsync(q => q.Id == id);
            return leaveAllocation;
        }

        public async Task<ICollection<LeaveAllocation>> GetLeaveAllocationByEmployee(string employeeId)
        {
            var period = DateTime.Now.Year;
            var alloc = await FindAll();
            return alloc.Where(q => q.EmployeeId == employeeId && q.Period == period).ToList();
        }

        public async Task<LeaveAllocation> GetLeaveAllocationByEmployeeAndLeaveType(string employeeId, int leaveTypeId)
        {
            var period = DateTime.Now.Year;
            var alloc = await FindAll();
            return alloc.FirstOrDefault(q => q.EmployeeId == employeeId && q.Period == period && q.LeaveTypeId == leaveTypeId);
        }

        public async Task<bool> isExists(int id)
        {
            var exists = await _db.LeaveAllocations.AnyAsync(q => q.Id == id);
            return exists;
        }

        public async Task<bool> Save()
        {
            var changes = await _db.SaveChangesAsync();
            return changes > 0;
        }

        public async Task<bool> Update(LeaveAllocation entity)
        {
            _db.LeaveAllocations.Update(entity);
            return await Save();
        }
    }
}
