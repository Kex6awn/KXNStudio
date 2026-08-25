using KxnPhotoStudio.Data;
using KxnPhotoStudio.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KxnPhotoStudio.Services.Implementations
{
    public class WorkflowNotificationService
        : IWorkflowNotificationService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public WorkflowNotificationService(
            AppDbContext context,
            IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task SendGalleryReadyEmailAsync(
            int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.SessionWorkflow)
                .FirstOrDefaultAsync(b =>
                    b.BookingId == bookingId);

            if (booking == null)
            {
                throw new InvalidOperationException(
                    "The booking could not be found.");
            }

            if (booking.SessionWorkflow == null)
            {
                throw new InvalidOperationException(
                    "The session workflow could not be found.");
            }

            if (!string.Equals(
                    booking.SessionWorkflow.EditingStatus,
                    "Completed",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Editing must be completed before sending a gallery-ready email.");
            }

            if (!string.Equals(
                    booking.SessionWorkflow.DeliveryStatus,
                    "Ready",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "The gallery must be marked Ready before sending this email.");
            }

            var subject =
                "Your Photos Are Ready - KXN Photo Studio";

            var body = BuildEmailTemplate(
                booking.FullName,
                "Your photos are ready!",
                $"Your photos from your {booking.ServiceType} session have finished editing and are ready for delivery.",
                booking.ServiceType,
                booking.EventDate,
                "Ready");

            await _emailService.SendEmailAsync(
                booking.Email,
                subject,
                body);
        }

        public async Task SendGalleryDeliveredEmailAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.SessionWorkflow)
                .FirstOrDefaultAsync(b =>
                    b.BookingId == bookingId);

            if (booking == null)
            {
                throw new InvalidOperationException(
                    "The booking could not be found.");
            }

            var workflow = booking.SessionWorkflow;

            if (workflow == null)
            {
                throw new InvalidOperationException(
                    "The session workflow could not be found.");
            }

            if (!string.Equals(
                    workflow.DeliveryStatus,
                    "Delivered",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "The gallery must be marked Delivered before sending this email.");
            }

            var subject =
                "Your Photo Gallery Has Been Delivered - KXN Photo Studio";

            var body = BuildEmailTemplate(
                booking.FullName,
                "Your photo gallery has been delivered!",
                $"Your photos from your {booking.ServiceType} session are now available.",
                booking.ServiceType,
                booking.EventDate,
                "Delivered",
                workflow.GalleryUrl,
                "Open Your Photo Gallery");

            await _emailService.SendEmailAsync(
                booking.Email,
                subject,
                body);
        }

        private string BuildEmailTemplate(
            string clientName,
            string heading,
            string message,
            string serviceType,
            DateTime eventDate,
            string statusText,
            string? actionUrl = null,
            string? actionText = null)
        {
            var formattedDate =
                eventDate.ToString("MMMM dd, yyyy");

            var actionSection =
                !string.IsNullOrWhiteSpace(actionUrl) &&
                !string.IsNullOrWhiteSpace(actionText)
                    ? $@"
                <table role=""presentation""
                       cellspacing=""0""
                       cellpadding=""0""
                       border=""0""
                       style=""margin:28px auto 0;"">
                    <tr>
                        <td style=""background-color:#111111;
                                   border-radius:8px;
                                   text-align:center;"">

                            <a href=""{actionUrl}""
                               style=""display:inline-block;
                                      padding:14px 24px;
                                      color:#ffffff;
                                      text-decoration:none;
                                      font-weight:bold;
                                      font-size:15px;"">
                                {actionText}
                            </a>

                        </td>
                    </tr>
                </table>"
                    : "";

            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset=""UTF-8"">
                <meta name=""viewport""
                      content=""width=device-width, initial-scale=1.0"">
                <title>KXN Photo Studio</title>
            </head>

            <body style=""margin:0;
                         padding:0;
                         background-color:#f4f4f4;
                         font-family:Arial, Helvetica, sans-serif;"">

                <table role=""presentation""
                       width=""100%""
                       cellspacing=""0""
                       cellpadding=""0""
                       border=""0""
                       style=""background-color:#f4f4f4;
                              padding:30px 15px;"">

                    <tr>
                        <td align=""center"">

                            <table role=""presentation""
                                   width=""100%""
                                   cellspacing=""0""
                                   cellpadding=""0""
                                   border=""0""
                                   style=""max-width:650px;
                                          background-color:#ffffff;
                                          border-radius:12px;
                                          overflow:hidden;
                                          box-shadow:0 4px 18px rgba(0,0,0,0.08);"">

                                <tr>
                                    <td style=""background-color:#111111;
                                               padding:32px 25px;
                                               text-align:center;"">

                                        <h1 style=""margin:0;
                                                   color:#ffffff;
                                                   font-size:28px;"">
                                            KXN Photo Studio
                                        </h1>

                                        <p style=""margin:10px 0 0;
                                                  color:#d6d6d6;
                                                  font-size:15px;"">
                                            Client Gallery Update
                                        </p>

                                    </td>
                                </tr>

                                <tr>
                                    <td style=""padding:35px 30px;"">

                                        <h2 style=""margin:0 0 15px;
                                                   color:#222222;
                                                   font-size:23px;"">
                                            Hi {clientName},
                                        </h2>

                                        <h3 style=""margin:0 0 15px;
                                                   color:#222222;
                                                   font-size:20px;"">
                                            {heading}
                                        </h3>

                                        <p style=""margin:0 0 28px;
                                                  color:#555555;
                                                  font-size:15px;
                                                  line-height:1.7;"">
                                            {message}
                                        </p>

                                        <table role=""presentation""
                                               width=""100%""
                                               cellspacing=""0""
                                               cellpadding=""0""
                                               border=""0""
                                               style=""border-collapse:collapse;
                                                      border:1px solid #e6e6e6;"">

                                            <tr>
                                                <td style=""padding:14px 16px;
                                                           background-color:#f8f8f8;
                                                           border-bottom:1px solid #e6e6e6;
                                                           font-weight:bold;
                                                           width:35%;"">
                                                    Service
                                                </td>

                                                <td style=""padding:14px 16px;
                                                           border-bottom:1px solid #e6e6e6;"">
                                                    {serviceType}
                                                </td>
                                            </tr>

                                            <tr>
                                                <td style=""padding:14px 16px;
                                                           background-color:#f8f8f8;
                                                           border-bottom:1px solid #e6e6e6;
                                                           font-weight:bold;"">
                                                    Session Date
                                                </td>

                                                <td style=""padding:14px 16px;
                                                           border-bottom:1px solid #e6e6e6;"">
                                                    {formattedDate}
                                                </td>
                                            </tr>

                                            <tr>
                                                <td style=""padding:14px 16px;
                                                           background-color:#f8f8f8;
                                                           font-weight:bold;"">
                                                    Status
                                                </td>

                                                <td style=""padding:14px 16px;"">

                                                    <span style=""display:inline-block;
                                                                 background-color:#d1e7dd;
                                                                 color:#0f5132;
                                                                 padding:6px 12px;
                                                                 border-radius:20px;
                                                                 font-size:13px;
                                                                 font-weight:bold;"">
                                                        {statusText}
                                                    </span>

                                                </td>
                                            </tr>

                                        </table>

                                        {actionSection}

                                        <p style=""margin:28px 0 0;
                                                  color:#555555;
                                                  font-size:15px;
                                                  line-height:1.7;"">
                                            Thank you for choosing KXN Photo Studio.
                                        </p>

                                        <p style=""margin:22px 0 0;
                                                  color:#222222;
                                                  font-size:15px;
                                                  line-height:1.7;"">
                                            Sincerely,<br>
                                            <strong>KXN Photo Studio</strong>
                                        </p>

                                    </td>
                                </tr>

                                <tr>
                                    <td style=""background-color:#111111;
                                               padding:22px;
                                               text-align:center;"">

                                        <p style=""margin:0;
                                                  color:#bdbdbd;
                                                  font-size:13px;"">
                                            KXN Photo Studio
                                        </p>

                                    </td>
                                </tr>

                            </table>

                        </td>
                    </tr>

                </table>

            </body>
            </html>";
        }
    }
}