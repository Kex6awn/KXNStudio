using KxnPhotoStudio.Models.ViewModels;
using KxnPhotoStudio.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KxnPhotoStudio.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class WorkflowController : Controller
    {
        private readonly ISessionWorkflowService _sessionWorkflowService;

        public WorkflowController(
            ISessionWorkflowService sessionWorkflowService)
        {
            _sessionWorkflowService = sessionWorkflowService;
        }

        public async Task<IActionResult> Index()
        {
            var workflows =
                await _sessionWorkflowService.GetAllAsync();

            var model = new WorkflowDashboardViewModel
            {
                NeedsEditingCount = workflows.Count(w =>
                    w.EditingStatus == "Not Started"),

                EditingCount = workflows.Count(w =>
                    w.EditingStatus == "In Progress"),

                ReadyToDeliverCount = workflows.Count(w =>
                    w.EditingStatus == "Completed" &&
                    w.DeliveryStatus == "Ready"),

                DeliveredCount = workflows.Count(w =>
                    w.DeliveryStatus == "Delivered"),

                Workflows = workflows
            };

            return View(model);
        }
    }
}