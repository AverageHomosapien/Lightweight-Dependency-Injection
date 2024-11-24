using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lightweight.Dependency.Injection
{
    internal sealed class DependencyRequest
    {
        public DependencyRequest(Type implementationType, DependencyLifetime scope)
        {
            ImplementationType = implementationType;
            Scope = scope;
        }

        public Type ImplementationType { get; set; }
        public DependencyLifetime Scope { get; set; }
    }
}
