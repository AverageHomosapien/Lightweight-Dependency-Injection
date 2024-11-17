using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjection
{
    internal sealed class DependencyImplementation
    {
        public DependencyImplementation(Type implementationType, DependencyScope scope)
        {
            ImplementationType = implementationType;
            Scope = scope;
        }

        public Type ImplementationType { get; set; }
        public DependencyScope Scope { get; set; }
    }
}
