using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Rinkudesu.Services.Links.Repositories.Exceptions
{
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class DataAlreadyExistsException : RepositoryException
    {
        public DataAlreadyExistsException() : base("Data was not added to the database because it already exists")
        {
        }

        public DataAlreadyExistsException(Guid duplicateKey) : base(
            $"Data with key {duplicateKey} was not added to the database because this key already exists")
        {
        }

        protected DataAlreadyExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}