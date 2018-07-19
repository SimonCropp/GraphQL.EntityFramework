using System;

class ErrorException : Exception
{
    public ErrorException(string message, Exception exception) : base(message,exception)
    {
    }
    public ErrorException(string message) : base(message)
    {
    }
}