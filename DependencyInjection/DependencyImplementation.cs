using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjection
{
    internal sealed class DependencyImplementation
    {
        public DependencyImplementation(Type implementationType, DependencyLifetime scope)
        {
            ImplementationType = implementationType;
            Scope = scope;
        }

        public Type ImplementationType { get; set; }
        public DependencyLifetime Scope { get; set; }
    }
}
