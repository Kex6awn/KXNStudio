using KxnPhotoStudio.Models;

namespace KxnPhotoStudio.Models.ViewModels
{
    public class WorkflowDashboardViewModel
    {
        public int NeedsEditingCount { get; set; }

        public int EditingCount { get; set; }

        public int ReadyToDeliverCount { get; set; }

        public int DeliveredCount { get; set; }

        public List<SessionWorkflow> Workflows { get; set; }
            = new();
    }
}