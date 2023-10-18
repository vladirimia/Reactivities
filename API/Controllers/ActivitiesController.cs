using Application.Activities;
using Domain;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ActivitiesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetActivities()
    {
        return Ok(await Mediator.Send(new List.Query()));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetActivity(Guid id)
    {
        return Ok(await Mediator.Send(new Details.Query { Id = id }));
    }

    [HttpPost]
    public async Task<IActionResult> Create(Activity activity)
    {
        await Mediator.Send(new Create.Command { Activity = activity });

        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Edit(Guid id, Activity activity)
    {
        activity.Id = id;

        await Mediator.Send(new Edit.Command { Activity = activity });

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await Mediator.Send(new Delete.Command { Id = id });

        return Ok();
    }
}
