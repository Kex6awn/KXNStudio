using Microsoft.EntityFrameworkCore;
using KxnPhotoStudio.Models;
using KxnPhotoStudio.Data;
using Microsoft.AspNetCore.Identity;
using KxnPhotoStudio.Services.Implementations;
using KxnPhotoStudio.Services.Interfaces;

namespace KxnPhotoStudio
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            // Dependency injection Kexhawn
            builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Identity Services (login/cookies/password hashing)
            builder.Services
                .AddIdentity<IdentityUser, IdentityRole>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;
                })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Admin/Auth/Login";
                options.LogoutPath = "/Admin/Auth/Logout";
            });

            builder.Services.Configure<EmailSettings>(
                builder.Configuration.GetSection("EmailSettings"));

            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<IClientService, ClientService>();
            builder.Services.AddScoped<IBookingService, BookingService>();
            builder.Services.AddScoped<IInvoiceService, InvoiceService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<IBookingEmailService, BookingEmailService>();
            builder.Services.AddScoped<IRazorViewRenderService, RazorViewRenderService>();
            builder.Services.AddScoped<IBookingStatusService, BookingStatusService>();
            builder.Services.AddScoped<ISessionWorkflowService, SessionWorkflowService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. I may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Authorization Enabled
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
                );

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
                );

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

                var adminEmail = builder.Configuration["AdminSettings:Email"];
                var adminPassword = builder.Configuration["AdminSettings:Password"];

                if (string.IsNullOrWhiteSpace(adminEmail) ||
                    string.IsNullOrWhiteSpace(adminPassword))
                {
                    throw new InvalidOperationException(
                        "Admin credentials are missing from User Secrets.");
                }

                var existingUser = await userManager.FindByEmailAsync(adminEmail);

                if (existingUser == null)
                {
                    var adminUser = new IdentityUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(adminUser, adminPassword);

                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine(error.Description);
                        }
                    }
                }
            }

            // Identity/Account/Login Enabled
            app.MapRazorPages();

            app.Run();
        }
    }
}
