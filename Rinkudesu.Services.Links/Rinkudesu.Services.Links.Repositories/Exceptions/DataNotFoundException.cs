using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Rinkudesu.Services.Links.Repositories.Exceptions
{
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class DataNotFoundException : RepositoryException
    {
        public DataNotFoundException() : base("No data matching the given conditions was found")
        {
        }

        public DataNotFoundException(Guid primaryKey) : base($"Data with primary key {primaryKey} was not found")
        {
        }

        protected DataNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}