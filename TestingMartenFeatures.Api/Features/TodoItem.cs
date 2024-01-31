namespace TestingMartenFeatures.Api.Features;

public record TodoItem
{
    public Guid Id { get; set; }
    public string Description { get; set; } = "";
    public int Counter { get; set; } = 0;
}

public record TodoItemAdded(Guid Id, string Description);

public record TodoItemDescriptionChanged(Guid Id, string Description);

public record TodoItemCounterIncremented(Guid Id, int Increment);
