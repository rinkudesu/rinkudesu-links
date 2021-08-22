using System;
using System.Diagnostics.CodeAnalysis;

namespace Rinkudesu.Services.Links.Repositories.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class DataNotfoundException : RepositoryException
    {
        public DataNotfoundException() : base("No data matching the given conditions was found")
        {
        }

        public DataNotfoundException(Guid primaryKey) : base($"Data with primary key {primaryKey} was not found")
        {
        }
    }
}