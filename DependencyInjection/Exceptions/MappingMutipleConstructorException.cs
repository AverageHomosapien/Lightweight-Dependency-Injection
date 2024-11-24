using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Lightweight.Dependency.Injection.Exceptions
{
    [Serializable]
    public class MappingMutipleConstructorException : Exception
    {
        public MappingMutipleConstructorException()
        {
        }

        public MappingMutipleConstructorException(string? message) : base(message)
        {
        }

        public MappingMutipleConstructorException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected MappingMutipleConstructorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
