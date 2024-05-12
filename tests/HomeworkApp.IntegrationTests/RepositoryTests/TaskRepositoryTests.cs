using FluentAssertions;
using HomeworkApp.Dal.Models;
using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.IntegrationTests.Creators;
using HomeworkApp.IntegrationTests.Fakers;
using HomeworkApp.IntegrationTests.Fixtures;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HomeworkApp.IntegrationTests.RepositoryTests;

[Collection(nameof(TestFixture))]
public class TaskRepositoryTests
{
    private readonly ITaskRepository _repository;

    public TaskRepositoryTests(TestFixture fixture)
    {
        _repository = fixture.TaskRepository;
    }

    [Fact]
    public async Task Add_Task_Success()
    {
        // Arrange
        const int count = 5;

        var tasks = TaskEntityV1Faker.Generate(count);

        // Act
        var results = await _repository.Add(tasks, default);

        // Asserts
        results.Should().HaveCount(count);
        results.Should().OnlyContain(x => x > 0);
    }

    [Fact]
    public async Task Get_SingleTask_Success()
    {
        // Arrange
        var tasks = TaskEntityV1Faker.Generate();
        var taskIds = await _repository.Add(tasks, default);
        var expectedTaskId = taskIds.First();
        var expectedTask = tasks.First()
            .WithId(expectedTaskId);

        // Act
        var results = await _repository.Get(new TaskGetModel()
        {
            TaskIds = new[] { expectedTaskId }
        }, default);

        // Asserts
        results.Should().HaveCount(1);
        var task = results.Single();

        task.Should().BeEquivalentTo(expectedTask);
    }

    [Fact]
    public async Task AssignTask_Success()
    {
        // Arrange
        var assigneeUserId = Create.RandomId();

        var tasks = TaskEntityV1Faker.Generate();
        var taskIds = await _repository.Add(tasks, default);
        var expectedTaskId = taskIds.First();
        var expectedTask = tasks.First()
            .WithId(expectedTaskId)
            .WithAssignedToUserId(assigneeUserId);
        var assign = AssignTaskModelFaker.Generate()
            .First()
            .WithTaskId(expectedTaskId)
            .WithAssignToUserId(assigneeUserId);

        // Act
        await _repository.Assign(assign, default);

        // Asserts
        var results = await _repository.Get(new TaskGetModel()
        {
            TaskIds = new[] { expectedTaskId }
        }, default);

        results.Should().HaveCount(1);
        var task = results.Single();

        expectedTask = expectedTask with { Status = assign.Status };
        task.Should().BeEquivalentTo(expectedTask);
    }

    [Fact]
    public async Task SetParentTask_Success()
    {
        //Arrange
        var tasks = TaskEntityV1Faker.Generate(2);
        var taskIds = await _repository.Add(tasks, default);
        var parentTaskId = taskIds.First();
        var childTaskId = taskIds.Last();

        //Act
        await _repository.SetParentTask(new SetParentTaskModel()
        {
            ParentTaskId = parentTaskId,
            TaskId = childTaskId
        }, default);

        var result = (await _repository.Get(new TaskGetModel()
        {
            TaskIds = new long[] { childTaskId }
        }, default))
        .Single();

        //Assert
        result.ParentTaskId.Should().Be(parentTaskId);
    }

    [Fact]
    public async Task GetSubTasksInStatus_Success()
    {
        // Arrange
        var childStatus = Dal.Enums.TaskStatus.InProgress;
        var subTask = TaskEntityV1Faker.Generate()
                    .Select(x => x.WithStatus((int)childStatus))
                    .ToArray();
        var subTaskId = (await _repository.Add(subTask, default))
            .Single();

        var tasks = TaskEntityV1Faker.Generate(10)
            .Select(x => x.WithStatus(1))
            .ToArray();
        var taskIds = await _repository.Add(tasks, default);

        var parentTaskId = taskIds.First();
        var childTaskId = taskIds.Skip(1).First();
        await _repository.SetParentTask(new SetParentTaskModel()
        {
            ParentTaskId = parentTaskId,
            TaskId = childTaskId
        }, default);

        await _repository.SetParentTask(new SetParentTaskModel()
        {
            ParentTaskId = childTaskId,
            TaskId = subTaskId
        }, default);

        // Act
        var result = await _repository.GetSubTasksInStatus(parentTaskId
            , new Dal.Enums.TaskStatus[] { childStatus }
            , default);

        // Assert
        result.Should().HaveCount(1);
        result[0].TaskId.Should().Be(subTaskId);
        result[0].Status.Should().Be(childStatus);
        result[0].ParentTaskIds.Should().BeEquivalentTo(new long[] { childTaskId, subTaskId });
    }
}
