namespace Forex.Application.Commons.Exceptions;

using FluentValidation.Results;
using System;
using System.Collections.Generic;

[Serializable]
internal class AppValidationException : Exception
{
    private List<ValidationFailure> failures;

    public AppValidationException()
    {
    }

    public AppValidationException(List<ValidationFailure> failures)
    {
        this.failures = failures;
    }

    public AppValidationException(string? message) : base(message)
    {
    }

    public AppValidationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}