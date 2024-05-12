namespace HomeworkApp.Dal.Repositories.Queries;

public static class TaskCommentRepositoryQueries
{
    public static string Add = @"
insert into task_comments (task_id, author_user_id, message, at)
values (@TaskID, @AuthorUserId, @Message, @At)
returning id;
";
    public static string SetDeletedAt = @"
update task_comments
   set deleted_at = @DeletedAt
 where id = @Id";

    public static string GetWithDeleted = @"
select *
  from task_comments tc
where tc.task_id = @TaskId";

    public static string GetWithoutDeleted = @"
select *
  from task_comments tc
where tc.task_id = @TaskId
  and deleted_at is null";

    public static string Update = @"
update task_comments
   set message = @Message
     , modified_at = @ModifiedAt
 where id = @Id;
";
}
