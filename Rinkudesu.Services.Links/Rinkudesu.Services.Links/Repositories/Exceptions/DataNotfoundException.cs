using System;

namespace Rinkudesu.Services.Links.Repositories.Exceptions
{
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