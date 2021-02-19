using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using leave_management.Contracts;
using leave_management.Data;
using leave_management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace leave_management.Controllers
{
    [Authorize(Roles ="Administrator")]
    public class LeaveAllocationsController : Controller
    {
        private readonly ILeaveTypeRepository _leaveTypeRepo;
        private readonly ILeaveAllocationRepository _leaveAllocationRepo;
        private readonly UserManager<Employee> _userManager;
        private readonly IMapper _mapper;

        public LeaveAllocationsController(ILeaveTypeRepository leaveTypeRepo, 
            ILeaveAllocationRepository leaveAllocationRepo, IMapper mapper, UserManager<Employee> userManager)
        {
            _leaveTypeRepo = leaveTypeRepo;
            _leaveAllocationRepo = leaveAllocationRepo;
            _userManager = userManager;
            _mapper = mapper;
        }

        // GET: LeaveAllocationsController
        public async Task<ActionResult> Index()
        {
            var leaveTypes = await _leaveTypeRepo.FindAll();
            var mappedLeaveType = _mapper.Map<List<LeaveType>, List<LeaveTypeVM>>(leaveTypes.ToList());
            var model = new CreateLeaveAllocationVM
            {
                LeaveTypes = mappedLeaveType,
                NumberUpdated = 0
            };

            return View(model);
        }

        public async Task<ActionResult> SetLeave(int id)
        {
            var leaveType = await _leaveTypeRepo.FindById(id);
            var employees = await _userManager.GetUsersInRoleAsync("Employee");

            foreach (var emp in employees)
            {
                if (await _leaveAllocationRepo.CheckAllocation(id, emp.Id))
                    continue;

                var allocation = new LeaveAllocationVM
                {
                    DateCreated = DateTime.Now,
                    EmployeeId = emp.Id,
                    LeaveTypeId = leaveType.Id,
                    NumberOfDays = leaveType.DefaultDays,
                    Period = DateTime.Now.Year
                };

                var leaveAllocation = _mapper.Map<LeaveAllocation>(allocation);
                await _leaveAllocationRepo.Create(leaveAllocation);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<ActionResult> ListEmployees()
        {
            var employees = await _userManager.GetUsersInRoleAsync("Employee");
            var model = _mapper.Map<List<EmployeeVM>>(employees);
            return View(model);
        }

        // GET: LeaveAllocationsController/Details/5
        public async Task<ActionResult> Details(string id)
        {
            var employee = _mapper.Map<EmployeeVM>(_userManager.FindByIdAsync(id).Result);
            var allocations = _mapper.Map<List<LeaveAllocationVM>>(await _leaveAllocationRepo.GetLeaveAllocationByEmployee(id));
            var model = new ViewsAllocationVM
            {
                Employee = employee,
                LeaveAllocations = allocations
            };
            return View(model);
        }

        // GET: LeaveAllocationsController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var leaveAllocation = await _leaveAllocationRepo.FindById(id);
            var model = _mapper.Map<EditLeaveAllocationVM>(leaveAllocation);
            return View(model);
        }

        // POST: LeaveAllocationsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditLeaveAllocationVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var record = await _leaveAllocationRepo.FindById(model.Id);
                record.NumberOfDays = model.NumberOfDays;
                var isSuccess = await _leaveAllocationRepo.Update(record);
                if (!isSuccess)
                {
                    ModelState.AddModelError("", "Backup error...");
                    return View(model);
                }
                return RedirectToAction(nameof(Details), new { id = model.EmployeeId});
            }
            catch
            {
                return View(model);
            }
        }

        // GET: LeaveAllocationsController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LeaveAllocationsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
