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
using Microsoft.AspNetCore.Mvc.Rendering;

namespace leave_management.Controllers
{
    [Authorize]
    public class LeaveRequestsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<Employee> _userManager;

        public LeaveRequestsController(IMapper mapper, IUnitOfWork unitOfWork, UserManager<Employee> userManager)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        // GET: LeaveRequestsController
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Index()
        {
            //var leaveRequests = await _leaveRequestRepo.FindAll();
            var leaveRequests = await _unitOfWork.leaveRequests.FindAll(includes: new List<string> { "RequestingEmployee", "LeaveType" });
            var leaveRequestModel = _mapper.Map<List<LeaveRequestVM>>(leaveRequests);
            var model = new AdminLeaveRequestViewVM
            {
                TotalRequests = leaveRequests.Count,
                ApprovedRequests = leaveRequests.Count(q => q.Approved == true),
                PendingRequests = leaveRequests.Count(q => q.Approved == null),
                RejectedRequests = leaveRequests.Count(q => q.Approved == false),
                LeaveRequests = leaveRequestModel
            };
            return View(model);
        }

        // GET: LeaveRequestsController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            //var leaveRequest = await _leaveRequestRepo.FindById(id);
            var leaveRequest = await _unitOfWork.leaveRequests.Find(q => q.Id == id, 
                                    includes: new List<string> { "RequestingEmployee", "ApprovedBy", "LeaveType" });
            var model = _mapper.Map<LeaveRequestVM>(leaveRequest);
            return View(model);
        }

        //Approve Request
        public async Task<ActionResult> ApproveRequest(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var leaveRequest = await _unitOfWork.leaveRequests.Find(q => q.Id == id);
                var employeeId = leaveRequest.RequestingEmployeeId;
                var leaveTypeId = leaveRequest.LeaveTypeId;
                var period = DateTime.Now.Year;

                //var allocation = await _leaveAllocationRepo.GetLeaveAllocationByEmployeeAndLeaveType(employeeId, leaveTypeId);
                var allocation = await _unitOfWork.leaveAllocations.Find(q => q.EmployeeId == employeeId && q.Period == period && q.LeaveTypeId == leaveTypeId);

                int daysRequested = (int)(leaveRequest.EndDate - leaveRequest.StartDate).TotalDays;
                allocation.NumberOfDays -= daysRequested;

                leaveRequest.Approved = true;
                leaveRequest.ApprovedById = user.Id;
                leaveRequest.DateActioned = DateTime.Now;

                _unitOfWork.leaveRequests.Update(leaveRequest);
                _unitOfWork.leaveAllocations.Update(allocation);
                await _unitOfWork.Save();

                return RedirectToAction(nameof(Index), "LeaveRequests");
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Index), "LeaveRequests");
            }
        }
        
        //Reject Request
        public async Task<ActionResult> RejectRequest(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var leaveRequest = await _unitOfWork.leaveRequests.Find(q => q.Id == id);
                leaveRequest.Approved = false;
                leaveRequest.ApprovedById = user.Id;
                leaveRequest.DateActioned = DateTime.Now;

                _unitOfWork.leaveRequests.Update(leaveRequest);
                await _unitOfWork.Save();
                return RedirectToAction(nameof(Index), "LeaveRequests");
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Index), "LeaveRequests");
            }
        }

        // GET: LeaveRequestsController/Create
        public async Task<ActionResult> Create()
        {
            
            var leaveTypes = await _unitOfWork.leaveTypes.FindAll();
            var leaveTypeItem = leaveTypes.Select(q => new SelectListItem
            {
                Text = q.Name,
                Value = q.Id.ToString()
            });

            var model = new CreateLeaveRequestVM
            {
                LeaveTypes = leaveTypeItem
            };
            return View(model);
        }

        // POST: LeaveRequestsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateLeaveRequestVM model)
        {
            try
            {
                var startDate = Convert.ToDateTime(model.StartDate);
                var endDate = Convert.ToDateTime(model.EndDate);

                var leaveTypes = await _unitOfWork.leaveTypes.FindAll();
                var leaveTypeItem = leaveTypes.Select(q => new SelectListItem
                {
                    Text = q.Name,
                    Value = q.Id.ToString()
                });

                model.LeaveTypes = leaveTypeItem;

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if (DateTime.Compare(startDate, endDate) > 1)
                {
                    ModelState.AddModelError("", "Start Date cannot be further in the future than the End Date");
                    return View(model);
                }

                var employee = await _userManager.GetUserAsync(User);
                var period = DateTime.Now.Year;

                //var allocation = await _leaveAllocationRepo.GetLeaveAllocationByEmployeeAndLeaveType(employee.Id, model.LeaveTypeId);
                var allocation = await _unitOfWork.leaveAllocations.Find(q => q.EmployeeId == employee.Id 
                                                                            && q.Period == period 
                                                                            && q.LeaveTypeId == model.LeaveTypeId);
                int daysRequested = (int)(endDate - startDate).TotalDays;
                int numberOfDays = allocation.NumberOfDays;
                if (allocation == null)
                {
                    ModelState.AddModelError("", "You Have No Days Left");
                }

                if (DateTime.Compare(startDate, endDate) > 1)
                {
                    ModelState.AddModelError("", "Start Date cannot be further in the future than the End Date");
                }

                if (daysRequested > allocation.NumberOfDays)
                {
                    ModelState.AddModelError("", "You Do Not Sufficient Days For This Request");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var leaveRequestModel = new LeaveRequestVM
                {
                    RequestingEmployeeId = employee.Id,
                    StartDate = startDate,
                    EndDate = endDate,
                    Approved = null,
                    DateRequested = DateTime.Now,
                    DateActioned = DateTime.Now,
                    LeaveTypeId = model.LeaveTypeId,
                    RequestComment = model.RequestComment
                };

                var leaveRequest = _mapper.Map<LeaveRequest>(leaveRequestModel);
                //var isSuccess = await _leaveRequestRepo.Create(leaveRequest);
                await _unitOfWork.leaveRequests.Create(leaveRequest);
                await _unitOfWork.Save();

                return RedirectToAction(nameof(Index),"Home");
            }
            catch
            {
                ModelState.AddModelError("", "Something went wrong...");
                return View();
            }
        }

        //Action for Employee
        public async Task<ActionResult> MyLeave()
        {
            var employee = await _userManager.GetUserAsync(User);
            var employeeId = employee.Id;

            //var empAllocations = await _leaveAllocationRepo.GetLeaveAllocationByEmployee(employeeId);
            var empAllocations = await _unitOfWork.leaveAllocations.FindAll(q => q.EmployeeId == employeeId, 
                                       includes: new List<string> { "LeaveType" });

            //var empRequests = await _leaveRequestRepo.GetLeaveRequestByEmployee(employeeId);
            var empRequests = await _unitOfWork.leaveRequests.FindAll(q => q.RequestingEmployeeId == employeeId);

            var empAllocModel = _mapper.Map<List<LeaveAllocationVM>>(empAllocations);
            var empRequestModel = _mapper.Map<List<LeaveRequestVM>>(empRequests);

            var model = new EmployeeLeaveRequestViewVM
            {
                LeaveAllocations = empAllocModel,
                LeaveRequests = empRequestModel
            };

            return View(model);
        }

        public async Task<ActionResult> Cancelled(int id)
        {
            //var leaveRequest = await _leaveRequestRepo.FindById(id);
            var leaveRequest = await _unitOfWork.leaveRequests.Find(q => q.Id == id);
            leaveRequest.Cancelled = true;
            //await _leaveRequestRepo.Update(leaveRequest);
            _unitOfWork.leaveRequests.Update(leaveRequest);
            await _unitOfWork.Save();
            return RedirectToAction("MyLeave");
        }

    }
}
