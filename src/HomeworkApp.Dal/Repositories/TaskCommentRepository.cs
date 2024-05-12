using Dapper;
using HomeworkApp.Dal.Entities;
using HomeworkApp.Dal.Models;
using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.Dal.Repositories.Queries;
using HomeworkApp.Dal.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HomeworkApp.Dal.Repositories;

public class TaskCommentRepository : PgRepository, ITaskCommentRepository
{
    public TaskCommentRepository(
        IOptions<DalOptions> dalSettings) : base(dalSettings.Value)
    {
    }

    public async Task<long> Add(TaskCommentEntityV1 model, CancellationToken token)
    {
        var cmd = new CommandDefinition(
            TaskCommentRepositoryQueries.Add,
            new
            {
                TaskId = model.TaskId,
                AuthorUserId = model.AuthorUserId,
                Message = model.Message,
                At = model.At
            }, cancellationToken: token);
        await using var connection = await GetConnection();
        return (await connection.QueryAsync<long>(cmd))
            .Single();
    }

    public async Task<TaskCommentEntityV1[]> Get(TaskCommentGetModel model, CancellationToken token)
    {
        var query = TaskCommentRepositoryQueries.GetWithDeleted;

        if (!model.IncludeDeleted)
        {
            query = TaskCommentRepositoryQueries.GetWithoutDeleted;
        }

        var cmd = new CommandDefinition(
            query,
            new
            {
                TaskId = model.TaskId
            }, cancellationToken: token);
        await using var connection = await GetConnection();
        return (await connection.QueryAsync<TaskCommentEntityV1>(cmd))
            .ToArray();
    }

    public async Task SetDeleted(long commentId, CancellationToken token)
    {

        var time = DateTimeOffset.UtcNow;

        var cmd = new CommandDefinition(
            TaskCommentRepositoryQueries.SetDeletedAt,
            new
            {
                DeletedAt = time,
                Id = commentId
            }, cancellationToken: token);
        await using var connection = await GetConnection();
        await connection.ExecuteAsync(cmd);
    }

    public async Task Update(TaskCommentEntityV1 model, CancellationToken token)
    {
        var modifiedAtTime = DateTimeOffset.UtcNow;

        var cmd = new CommandDefinition(
            TaskCommentRepositoryQueries.Update,
            new
            {
                Id = model.Id,
                TaskId = model.TaskId,
                AuthorUserId = model.AuthorUserId,
                Message = model.Message,
                At = model.At,
                ModifiedAt = modifiedAtTime
            }, cancellationToken: token);
        await using var connection = await GetConnection();
        await connection.ExecuteAsync(cmd);
    }
}
