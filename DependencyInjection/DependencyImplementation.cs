using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lightweight.Dependency.Injection
{
    internal sealed class DependencyImplementation
    {
        public DependencyImplementation(Func<object> containedObject, Type objectType, DependencyLifetime dependencyScope)
        {
            ContainedObject = containedObject;
            ObjectType = objectType;
            DependencyScope = dependencyScope;
        }

        /// <summary>
        /// We store our dependencies as a Func rather than the object itself. If we stored the object rather than a Func, the Transient scope
        /// would always return the same object, as the object would be created once and then stored in the dictionary. A Func allows us some Func-y
        /// functionality
        /// </summary>
        public Func<object> ContainedObject { get; }
        public Type ObjectType { get; }
        public DependencyLifetime DependencyScope { get; }
    }
}
