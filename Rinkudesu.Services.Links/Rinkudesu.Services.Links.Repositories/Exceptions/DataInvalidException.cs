using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Rinkudesu.Services.Links.Repositories.Exceptions;

[Serializable]
[ExcludeFromCodeCoverage]
public class DataInvalidException : RepositoryException
{
    public DataInvalidException() : base("No data matching the given conditions was found")
    {
    }

    public DataInvalidException(string message) : base(message)
    {
    }

    public DataInvalidException(Guid primaryKey) : base($"Data with primary key {primaryKey} was invalid for the given context")
    {
    }

    public DataInvalidException(Guid primaryKey, string reason) : base($"Data with primary key {primaryKey} was invalid for the following reason: {reason}")
    {
    }

    protected DataInvalidException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
