using FluentAssertions;
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
}