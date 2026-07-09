using System.Threading.Tasks;
using AcademicGPA.Application.Features.GradeScale.Commands.CreateGradeScale;
using AcademicGPA.Application.Features.GradeScale.Queries.GetGradeScales;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGPA.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/grade-scales")]
public class GradeScalesController : ControllerBase
{
    private readonly IMediator _mediator;

    public GradeScalesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetGradeScales()
    {
        var result = await _mediator.Send(new GetGradeScalesQuery());
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    public async Task<IActionResult> CreateGradeScale([FromBody] CreateGradeScaleCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(new { success = true, data = new { id } });
    }
}
