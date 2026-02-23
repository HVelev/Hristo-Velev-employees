using EmployeeCollaborationUI.Interfaces;
using EmployeeCollaborationUI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IEmployeeCollaborationService, EmployeeCollaborationService>();

var app = builder.Build();


app.UseStaticFiles();
app.UseRouting();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.Run();
