using Dapper;
using HomeworkApp.Dal.Entities;
using HomeworkApp.Dal.Models;
using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.Dal.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HomeworkApp.Dal.Repositories;

public class TaskRepository : PgRepository, ITaskRepository
{
    public TaskRepository(
        IOptions<DalOptions> dalSettings) : base(dalSettings.Value)
    {
    }

    public async Task<long[]> Add(TaskEntityV1[] tasks, CancellationToken token)
    {
        const string sqlQuery = @"
insert into tasks (parent_task_id, number, title, description, status, created_at, created_by_user_id, assigned_to_user_id, completed_at) 
select parent_task_id, number, title, description, status, created_at, created_by_user_id, assigned_to_user_id, completed_at
  from UNNEST(@Tasks)
returning id;
";

        await using var connection = await GetConnection();
        var ids = await connection.QueryAsync<long>(
            new CommandDefinition(
                sqlQuery,
                new
                {
                    Tasks = tasks
                },
                cancellationToken: token));

        return ids
            .ToArray();
    }

    public async Task<TaskEntityV1[]> Get(TaskGetModel query, CancellationToken token)
    {
        var baseSql = @"
select id
     , parent_task_id
     , number
     , title
     , description
     , status
     , created_at
     , created_by_user_id
     , assigned_to_user_id
     , completed_at
  from tasks
";

        var conditions = new List<string>();
        var @params = new DynamicParameters();

        if (query.TaskIds.Any())
        {
            conditions.Add($"id = ANY(@TaskIds)");
            @params.Add($"TaskIds", query.TaskIds);
        }

        var cmd = new CommandDefinition(
            baseSql + $" WHERE {string.Join(" AND ", conditions)} ",
            @params,
            commandTimeout: DefaultTimeoutInSeconds,
            cancellationToken: token);

        await using var connection = await GetConnection();
        return (await connection.QueryAsync<TaskEntityV1>(cmd))
            .ToArray();
    }

    public async Task Assign(AssignTaskModel model, CancellationToken token)
    {
        const string sqlQuery = @"
update tasks
   set assigned_to_user_id = @AssignToUserId
     , status = @Status
 where id = @TaskId
";

        await using var connection = await GetConnection();
        await connection.ExecuteAsync(
            new CommandDefinition(
                sqlQuery,
                new
                {
                    TaskId = model.TaskId,
                    AssignToUserId = model.AssignToUserId,
                    Status = model.Status
                },
                cancellationToken: token));
    }

    public async Task<SubTaskModel[]> GetSubTasksInStatus(long parentTaskId, Enums.TaskStatus[] statuses, CancellationToken token)
    {
        var sqlQuery = @"
with recursive task_tree
  as (select t.id
           , t.parent_task_id
           , array[]::BIGINT[] as path
        from tasks t
       where t.id = @ParentTaskId
       union all 
       select t.id
            , t.parent_task_id
            , tt.path || t.id as path
         from task_tree tt
         join tasks t on t.parent_task_id = tt.id)
select t.id as TaskId
     , t.title as Title
     , t.status as Status
     , tt.path as ParentTaskIds 
  from task_tree tt
  join tasks t on t.id = tt.id
 where array_length(tt.path, 1) <> 0
";
        var @params = new DynamicParameters();
        @params.Add("ParentTaskId", parentTaskId);
        if (statuses.Any())
        {
            sqlQuery += @" and t.status = any(@SubTaskStatuses)";
            var intStatuses = Array.ConvertAll(statuses, v => (int)v);
            @params.Add("SubTaskStatuses", intStatuses);
        }

        var cmd = new CommandDefinition(
            sqlQuery,
            @params,
            cancellationToken: token);

        await using var connection = await GetConnection();
        return (await connection.QueryAsync<SubTaskModel>(cmd))
            .ToArray();
    }

    public async Task SetParentTask(SetParentTaskModel model, CancellationToken token)
    {
        string sqlQuery = @"
update tasks
   set parent_task_id = @ParentTaskId
 where id = @TaskId
";

        var cmd = new CommandDefinition(
            sqlQuery,
            new
            {
                ParentTaskId = model.ParentTaskId,
                TaskId = model.TaskId
            },
            cancellationToken: token);

        await using var connection = await GetConnection();
        await connection.ExecuteAsync(cmd);
    }
}