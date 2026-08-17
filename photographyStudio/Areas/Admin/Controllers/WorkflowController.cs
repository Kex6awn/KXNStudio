using KxnPhotoStudio.Models;
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
        private readonly IJobCompletionService _jobCompletionService;

        public WorkflowController(
            ISessionWorkflowService sessionWorkflowService,
            IJobCompletionService jobCompletionService)
        {
            _sessionWorkflowService = sessionWorkflowService;
            _jobCompletionService = jobCompletionService;
        }

        public async Task<IActionResult> Index()
        {
            var workflows =
                await _sessionWorkflowService.GetAllAsync();

            var activeWorkflows =
                new List<SessionWorkflow>();

            var completedWorkflows =
                new List<SessionWorkflow>();

            foreach (var workflow in workflows)
            {
                var completion =
                    await _jobCompletionService
                        .GetJobCompletionAsync(workflow.BookingId);

                if (completion.IsJobComplete)
                {
                    completedWorkflows.Add(workflow);
                }
                else
                {
                    activeWorkflows.Add(workflow);
                }
            }

            var model = new WorkflowDashboardViewModel
            {
                NeedsEditingCount = activeWorkflows.Count(w =>
                    string.Equals(
                        w.EditingStatus,
                        "Not Started",
                        StringComparison.OrdinalIgnoreCase)),

                EditingCount = activeWorkflows.Count(w =>
                    string.Equals(
                        w.EditingStatus,
                        "In Progress",
                        StringComparison.OrdinalIgnoreCase)),

                ReadyToDeliverCount = activeWorkflows.Count(w =>
                    string.Equals(
                        w.EditingStatus,
                        "Completed",
                        StringComparison.OrdinalIgnoreCase)
                    &&
                    string.Equals(
                        w.DeliveryStatus,
                        "Ready",
                        StringComparison.OrdinalIgnoreCase)),

                DeliveredCount = workflows.Count(w =>
                    string.Equals(
                        w.DeliveryStatus,
                        "Delivered",
                        StringComparison.OrdinalIgnoreCase)),

                ActiveJobsCount = activeWorkflows.Count,

                CompletedJobsCount = completedWorkflows.Count,

                ActiveWorkflows = activeWorkflows,

                CompletedWorkflows = completedWorkflows
            };

            return View(model);
        }
    }
}