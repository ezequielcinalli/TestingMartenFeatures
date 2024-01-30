namespace TestingMartenFeatures.Api.Features;

public record TodoItem
{
    public Guid Id { get; set; }
    public string Description { get; set; } = "";
}

public record TodoItemAdded(Guid Id, string Description);

public record TodoItemDescriptionChanged(Guid Id, string Description);