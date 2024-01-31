using Marten;
using MediatR;

namespace TestingMartenFeatures.Api.Features;

public record IncrementTodoItemCounter(Guid Id, int Increment) : IRequest<TodoItem>;

public class IncrementTodoItemCounterHandler(IDocumentSession session)
    : IRequestHandler<IncrementTodoItemCounter, TodoItem>
{
    public async Task<TodoItem> Handle(IncrementTodoItemCounter request, CancellationToken cancellationToken)
    {
        // var todoItem = await session.Events.AggregateStreamAsync<TodoItem>(request.Id, token: cancellationToken);
        var todoItemStream = await session.Events.FetchForWriting<TodoItem>(request.Id, cancellationToken);
        var todoItem = todoItemStream.Aggregate;

        if (todoItem is null) throw new ArgumentException("TodoItem not found");
        
        await Task.Delay(500, cancellationToken); //needed to simulate some concurrency
        
        if (todoItem.Counter + request.Increment >= 10)
            throw new ArgumentException("TodoItem Counter reaches 10");

        var @event = new TodoItemCounterIncremented(todoItem.Id, request.Increment);
        // session.Events.Append(@event.Id, @event);
        todoItemStream.AppendOne(@event);
        await session.SaveChangesAsync(cancellationToken);

        return todoItem with { Counter = todoItem.Counter + request.Increment };
    }
}