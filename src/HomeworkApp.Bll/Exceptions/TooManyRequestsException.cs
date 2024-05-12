using System;

namespace HomeworkApp.Bll.Exceptions;

public class TooManyRequestsException : Exception
{
    public TooManyRequestsException() : base() { }
    public TooManyRequestsException(string message) : base(message) { }
    public TooManyRequestsException(string message, Exception innerException) : base(message, innerException) { }
}
