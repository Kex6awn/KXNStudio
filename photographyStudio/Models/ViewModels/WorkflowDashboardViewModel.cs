using KxnPhotoStudio.Models;

namespace KxnPhotoStudio.Models.ViewModels
{
    public class WorkflowDashboardViewModel
    {
        public int NeedsEditingCount { get; set; }

        public int EditingCount { get; set; }

        public int ReadyToDeliverCount { get; set; }

        public int DeliveredCount { get; set; }

        public int ActiveJobsCount { get; set; }

        public int CompletedJobsCount { get; set; }

        public string? Search { get; set; }

        public string? StatusFilter { get; set; }

        public List<SessionWorkflow> ActiveWorkflows { get; set; }
            = new();

        public List<SessionWorkflow> CompletedWorkflows { get; set; }
            = new();
    }
}