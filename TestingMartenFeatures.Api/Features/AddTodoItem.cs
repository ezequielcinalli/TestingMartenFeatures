using Marten;
using MediatR;

namespace TestingMartenFeatures.Api.Features;

public record AddTodoItem(string Description) : IRequest<TodoItem>;

public class AddTodoItemHandler(IDocumentSession session) : IRequestHandler<AddTodoItem, TodoItem>
{
    public async Task<TodoItem> Handle(AddTodoItem request, CancellationToken cancellationToken)
    {
        var todoItem = new TodoItem
        {
            Id = Guid.NewGuid(),
            Description = request.Description
        };

        var @event = new TodoItemAdded(todoItem.Id, todoItem.Description);

        session.Events.StartStream<TodoItem>(@event.Id, @event);
        await session.SaveChangesAsync(cancellationToken);

        return todoItem;
    }
}