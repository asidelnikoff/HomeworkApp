using FluentMigrator;

namespace HomeworkApp.Dal.Migrations;

[Migration(20240409102200, TransactionBehavior.None)]
public class ModifyTaskComments : Migration
{
    public override void Up()
    {
        const string addModifiedAt = @"
alter table task_comments
  add column if not exists modified_at timestamp with time zone null;
";
        const string addDeletedAt = @"
alter table task_comments
  add column if not exists deleted_at timestamp with time zone null;
";
        Execute.Sql(addModifiedAt);
        Execute.Sql(addDeletedAt);
    }

    public override void Down()
    {
        const string deleteModifiedAt = @"
 alter table task_comments
drop column if exists modified_at;
";
        const string deleteDeletedAt = @"
 alter table task_comments
drop column if exists deleted_at;
";
        Execute.Sql(deleteModifiedAt);
        Execute.Sql(deleteDeletedAt);
    }
}
