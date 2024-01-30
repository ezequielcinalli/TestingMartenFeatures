using Marten;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace TestingMartenFeatures.Api.Features;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Test([FromServices] IDocumentStore store, CancellationToken cancellationToken)
    {
        await using var session = store.LightweightSession();
        var event1 = new TodoItemAdded(Guid.NewGuid(), "item1");
        var event2 = new TodoItemDescriptionChanged(event1.Id, "item1 updated");

        session.Events.StartStream<TodoItem>(event1.Id, event1);
        await session.SaveChangesAsync(cancellationToken);

        var todoItem1 = await session.Events.FetchForWriting<TodoItem>(event2.Id, cancellationToken);
        todoItem1.AppendOne(event2);
        await session.SaveChangesAsync(cancellationToken);

        var todoItemStream = await session.Events.FetchForWriting<TodoItem>(event2.Id, cancellationToken);
        //Here todoItemStream.Aggregate have description = "item1" instead of "item1 updated"

        await using var session2 = store.LightweightSession();
        var todoItemStream2 = await session2.Events.FetchForWriting<TodoItem>(event2.Id, cancellationToken);
        //Here todoItemStream2.Aggregate have the correct description = "item1 updated"

        return Ok("All Ok!");
    }

    [HttpPost("add-todoitem")]
    public async Task<IActionResult> AddTodoItem([FromServices] IMediator mediator,
        [FromBody] AddTodoItem request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("change-todoitem-description")]
    public async Task<IActionResult> ChangeTodoItemDescription([FromServices] IMediator mediator,
        [FromBody] ChangeTodoItemDescription request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("delete-all-database-data")]
    public async Task<IActionResult> DeleteDatabaseData([FromServices] IDocumentStore store,
        CancellationToken cancellationToken)
    {
        await store.Advanced.Clean.CompletelyRemoveAllAsync(cancellationToken);

        return Ok("All Ok!");
    }
}