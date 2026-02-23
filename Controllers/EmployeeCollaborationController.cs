using Microsoft.AspNetCore.Mvc;
using EmployeeCollaborationUI.Interfaces;

namespace EmployeeCollaborationUI.Controllers;

[ApiController]
[Route("[controller]")]
public class EmployeeCollaborationController : ControllerBase
{
    private readonly IEmployeeCollaborationService service;

    public EmployeeCollaborationController(IEmployeeCollaborationService service)
    {
        this.service = service;
    }

    [HttpPost("FetchLongestCollaborationWorkers")]
    public IActionResult FetchLongestCollaborationWorkers([FromForm] IFormFile file, [FromForm] string format)
    {
        var assignment = this.service.ReadAssignments(file, format);

        return Ok(this.service.FindLongestCollaboration(assignment));
    }
}
