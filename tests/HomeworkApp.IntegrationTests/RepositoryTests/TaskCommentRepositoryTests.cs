using FluentAssertions;
using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.IntegrationTests.Fakers;
using HomeworkApp.IntegrationTests.Fixtures;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HomeworkApp.IntegrationTests.RepositoryTests;

[Collection(nameof(TestFixture))]
public class TaskCommentRepositoryTests
{
    private readonly ITaskCommentRepository _repository;

    public TaskCommentRepositoryTests(TestFixture fixture)
    {
        _repository = fixture.TaskCommentRepository;
    }

    [Fact]
    public async Task Add_Comment_Success()
    {
        //Arrange
        var comment = TaskCommentEntityV1Faker.Generate()
            .Single();

        //Act
        var result = await _repository.Add(comment, default);

        //Assert
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Get_WithoutDeleted_Success()
    {
        //Arrange
        var comment = TaskCommentEntityV1Faker.Generate()
            .Single();
        var expectedCommentId = await _repository.Add(comment, default);
        var expectedComment = comment.WithId(expectedCommentId);

        //Act
        var result = await _repository.Get(new Dal.Models.TaskCommentGetModel
        {
            TaskId = comment.TaskId,
            IncludeDeleted = false
        }, default);

        //Assert
        result.Should().HaveCount(1);

        var resultComment = result.Single();
        resultComment.Should().BeEquivalentTo(expectedComment);
    }

    [Fact]
    public async Task SetDeleted_Success()
    {
        //Arrange
        var comment = TaskCommentEntityV1Faker.Generate()
            .Single();
        var expectedCommentId = await _repository.Add(comment, default);

        var expectedTime = DateTimeOffset.UtcNow;

        //Act
        await _repository.SetDeleted(expectedCommentId, default);

        //Assert
        var result = await _repository.Get(new Dal.Models.TaskCommentGetModel()
        {
            TaskId = comment.TaskId,
            IncludeDeleted = true
        }, default);

        result.Should().HaveCount(1);

        var resultComment = result.Single();
        resultComment.DeletedAt.Should().BeOnOrAfter(expectedTime);
    }

    [Fact]
    public async Task Update_Success()
    {
        //Arrange
        var comment = TaskCommentEntityV1Faker.Generate()
            .Single();
        var expectedCommentId = await _repository.Add(comment, default);

        var modifiedComment = comment
            .WithId(expectedCommentId)
            .WithTaskId(comment.TaskId)
            .WithMessage("new message");

        var modifiedAt = DateTimeOffset.UtcNow;

        //Act
        await _repository.Update(modifiedComment, default);

        //Assert
        var result = await _repository.Get(new Dal.Models.TaskCommentGetModel()
        {
            TaskId = modifiedComment.TaskId,
            IncludeDeleted = false
        }, default);

        result.Should().HaveCount(1);

        var resultComment = result.Single();
        resultComment.ModifiedAt.Should().BeOnOrAfter(modifiedAt);

        modifiedComment = modifiedComment.WithModifiedAt(resultComment.ModifiedAt);
        resultComment.Should().BeEquivalentTo(modifiedComment);
    }
}
