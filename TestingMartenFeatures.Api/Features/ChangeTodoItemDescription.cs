using Marten;
using MediatR;

namespace TestingMartenFeatures.Api.Features;

public record ChangeTodoItemDescription(Guid Id, string Description) : IRequest<TodoItem>;

public class ChangeTodoItemDescriptionHandler(IDocumentSession session)
    : IRequestHandler<ChangeTodoItemDescription, TodoItem>
{
    public async Task<TodoItem> Handle(ChangeTodoItemDescription request, CancellationToken cancellationToken)
    {
        var todoItem = await session.Events.FetchForWriting<TodoItem>(request.Id, cancellationToken);

        if (todoItem.Aggregate is null) throw new ArgumentException("TodoItem not found");
        if (todoItem.Aggregate.Description == request.Description)
            throw new ArgumentException("TodoItem description is the same");

        var @event = new TodoItemDescriptionChanged(todoItem.Id, request.Description);
        todoItem.AppendOne(@event);
        await session.SaveChangesAsync(cancellationToken);

        return todoItem.Aggregate with { Description = request.Description };
    }
}