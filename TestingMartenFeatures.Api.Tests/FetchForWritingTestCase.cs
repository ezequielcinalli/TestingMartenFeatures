using FluentAssertions;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using TestingMartenFeatures.Api.Features;

namespace TestingMartenFeatures.Api.Tests;

public class FetchForWritingTestCase(TestApplicationFactory testApplicationFactory)
    : IClassFixture<TestApplicationFactory>
{
    [Fact]
    public async Task AddTodoItem_Returns_TodoItem()
    {
        var request = new AddTodoItem("item1");

        var result = await testApplicationFactory.SendAsync(request);

        result.Description.Should().Be("item1");
    }

    [Fact]
    public async Task ChangeTodoItem_Returns_TodoItem()
    {
        var request = new AddTodoItem("item1");
        var result = await testApplicationFactory.SendAsync(request);

        var requestChange = new ChangeTodoItemDescription(result.Id, "item1 updated");
        var resultChange = await testApplicationFactory.SendAsync(requestChange);

        resultChange.Description.Should().Be("item1 updated");
    }

    [Fact]
    public async Task ChangeTodoItem_DescriptionIsTheSame_ThrowsException()
    {
        var request = new AddTodoItem("item1");
        var result = await testApplicationFactory.SendAsync(request);

        var requestChange = new ChangeTodoItemDescription(result.Id, "item1 updated");
        await testApplicationFactory.SendAsync(requestChange);

        var action = () => testApplicationFactory.SendAsync(requestChange);
        await action.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ChangeTodoItem_DescriptionIsTheSameAndForceNewScope_ThrowsException()
    {
        var request = new AddTodoItem("item1");
        var result = await testApplicationFactory.SendAsync(request);

        var requestChange = new ChangeTodoItemDescription(result.Id, "item1 updated");
        await testApplicationFactory.SendAsync(requestChange);

        var action = () => testApplicationFactory.SendAsync(requestChange, true);
        await action.Should().ThrowAsync<Exception>();
    }
    
    [Fact]
    public async Task IncrementTodoItemCounter_CounterReaches10_NotShouldUpdateCounter()
    {
        var request = new AddTodoItem("delay");
        var result = await testApplicationFactory.SendAsync(request);

        var requestIncrement = new IncrementTodoItemCounter(result.Id, 1);
        var requestIncrementHigh = new IncrementTodoItemCounter(result.Id, 9);
        
        var task1 = testApplicationFactory.SendAsync(requestIncrement,true);
        await Task.Delay(300); //needed to simulate some concurrency
        var task2 = testApplicationFactory.SendAsync(requestIncrementHigh,true);
        
        var tasks = new List<Task> {task1, task2};
        await Task.WhenAll(tasks);

        using var scope = testApplicationFactory.Services.CreateScope();
        await using var querySession =scope.ServiceProvider.GetRequiredService<IQuerySession>();
        var todoFromDb = querySession.Query<TodoItem>().FirstOrDefault(x => x.Id == result.Id);
        Assert.NotNull(todoFromDb);
        todoFromDb.Counter.Should().BeLessThan(10);
    }
}