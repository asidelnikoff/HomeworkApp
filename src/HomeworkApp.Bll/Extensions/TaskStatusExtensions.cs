using System;

namespace HomeworkApp.Bll.Extensions;

public static class TaskStatusExtensions
{
    public static Dal.Enums.TaskStatus ToDal(this Enums.TaskStatus src)
    {
        return src switch
        {
            Enums.TaskStatus.Draft => Dal.Enums.TaskStatus.Draft,
            Enums.TaskStatus.ToDo => Dal.Enums.TaskStatus.ToDo,
            Enums.TaskStatus.InProgress => Dal.Enums.TaskStatus.InProgress,
            Enums.TaskStatus.Done => Dal.Enums.TaskStatus.Done,
            Enums.TaskStatus.Canceled => Dal.Enums.TaskStatus.Canceled,
            _ => throw new ArgumentOutOfRangeException(nameof(src), src, null)
        };
    }
}
