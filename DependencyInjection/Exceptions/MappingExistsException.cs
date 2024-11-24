using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Lightweight.Dependency.Injection.Exceptions
{
    [Serializable]
    public class MappingExistsException : Exception
    {
        public MappingExistsException()
        {
        }

        public MappingExistsException(string? message) : base(message)
        {
        }

        public MappingExistsException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected MappingExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
