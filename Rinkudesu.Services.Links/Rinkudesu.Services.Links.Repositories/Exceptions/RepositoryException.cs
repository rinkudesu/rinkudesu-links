using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Rinkudesu.Services.Links.Repositories.Exceptions
{
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class RepositoryException : Exception
    {
        public RepositoryException()
        {
        }

        public RepositoryException(string message) : base(message)
        {
        }

        public RepositoryException(string message, Exception inner) : base(message, inner)
        {
        }

        protected RepositoryException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}