using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Rinkudesu.Services.Links.Repositories.Exceptions
{
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class DataNotfoundException : RepositoryException
    {
        public DataNotfoundException() : base("No data matching the given conditions was found")
        {
        }

        public DataNotfoundException(Guid primaryKey) : base($"Data with primary key {primaryKey} was not found")
        {
        }

        protected DataNotfoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}