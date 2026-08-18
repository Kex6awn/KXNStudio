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

        public async Task<IActionResult> Index(
            string? search,
            string? statusFilter)
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

            // -----------------------------------------
            // OVERALL COUNTS BEFORE FILTERING
            // -----------------------------------------

            var needsEditingCount = activeWorkflows.Count(w =>
                string.Equals(
                    w.EditingStatus,
                    "Not Started",
                    StringComparison.OrdinalIgnoreCase));

            var editingCount = activeWorkflows.Count(w =>
                string.Equals(
                    w.EditingStatus,
                    "In Progress",
                    StringComparison.OrdinalIgnoreCase));

            var readyToDeliverCount = activeWorkflows.Count(w =>
                string.Equals(
                    w.EditingStatus,
                    "Completed",
                    StringComparison.OrdinalIgnoreCase)
                &&
                string.Equals(
                    w.DeliveryStatus,
                    "Ready",
                    StringComparison.OrdinalIgnoreCase));

            var deliveredCount = workflows.Count(w =>
                string.Equals(
                    w.DeliveryStatus,
                    "Delivered",
                    StringComparison.OrdinalIgnoreCase));

            var activeJobsCount = activeWorkflows.Count;

            var completedJobsCount = completedWorkflows.Count;


            // -----------------------------------------
            // SEARCH
            // -----------------------------------------

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalizedSearch = search.Trim();

                activeWorkflows = activeWorkflows
                    .Where(w =>
                        w.Booking.FullName.Contains(
                            normalizedSearch,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        w.Booking.Email.Contains(
                            normalizedSearch,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        w.Booking.ServiceType.Contains(
                            normalizedSearch,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();

                completedWorkflows = completedWorkflows
                    .Where(w =>
                        w.Booking.FullName.Contains(
                            normalizedSearch,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        w.Booking.Email.Contains(
                            normalizedSearch,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        w.Booking.ServiceType.Contains(
                            normalizedSearch,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }


            // -----------------------------------------
            // STATUS FILTER
            // -----------------------------------------

            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                activeWorkflows = statusFilter switch
                {
                    "NeedsEditing" => activeWorkflows
                        .Where(w =>
                            string.Equals(
                                w.EditingStatus,
                                "Not Started",
                                StringComparison.OrdinalIgnoreCase))
                        .ToList(),

                    "Editing" => activeWorkflows
                        .Where(w =>
                            string.Equals(
                                w.EditingStatus,
                                "In Progress",
                                StringComparison.OrdinalIgnoreCase))
                        .ToList(),

                    "ReadyToDeliver" => activeWorkflows
                        .Where(w =>
                            string.Equals(
                                w.EditingStatus,
                                "Completed",
                                StringComparison.OrdinalIgnoreCase)
                            &&
                            string.Equals(
                                w.DeliveryStatus,
                                "Ready",
                                StringComparison.OrdinalIgnoreCase))
                        .ToList(),

                    "Delivered" => activeWorkflows
                        .Where(w =>
                            string.Equals(
                                w.DeliveryStatus,
                                "Delivered",
                                StringComparison.OrdinalIgnoreCase))
                        .ToList(),

                    _ => activeWorkflows
                };
            }


            // -----------------------------------------
            // VIEW MODEL
            // -----------------------------------------

            var model = new WorkflowDashboardViewModel
            {
                NeedsEditingCount = needsEditingCount,

                EditingCount = editingCount,

                ReadyToDeliverCount = readyToDeliverCount,

                DeliveredCount = deliveredCount,

                ActiveJobsCount = activeJobsCount,

                CompletedJobsCount = completedJobsCount,

                Search = search,

                StatusFilter = statusFilter,

                ActiveWorkflows = activeWorkflows,

                CompletedWorkflows = completedWorkflows
            };

            return View(model);
        }
    }
}