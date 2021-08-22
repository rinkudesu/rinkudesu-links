using System;
using System.Diagnostics.CodeAnalysis;

namespace Rinkudesu.Services.Links.Repositories.Exceptions
{
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
    }
}