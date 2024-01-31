using Marten.Events.Aggregation;

namespace TestingMartenFeatures.Api.Features;

public class TodoItemProjection : SingleStreamProjection<TodoItem>
{
    public void Apply(TodoItemAdded @event, TodoItem document)
    {
        document.Id = @event.Id;
        document.Description = @event.Description;
    }

    public void Apply(TodoItemDescriptionChanged @event, TodoItem document)
    {
        document.Description = @event.Description;
    }
    
    public void Apply(TodoItemCounterIncremented @event, TodoItem document)
    {
        document.Counter += @event.Increment;
    }
}