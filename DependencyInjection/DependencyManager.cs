using System.Runtime.Remoting;

namespace DependencyInjection
{
    public class DependencyManager
    {
        private Dictionary<Type, DependencyImplementation> dependentInterfaces = new();

        /// <summary>
        /// Holds a set of active instances of dependency interfaces
        /// </summary>
        private Dictionary<Type, object> activeInterfaces = new();

        public void AddSingleton<TInterface, TImplementation>() where TInterface : class
                                                             where TImplementation : class, TInterface        
        {
            if (dependentInterfaces.ContainsKey(typeof(TInterface)))
            {
                throw new MappingExistsException($"Mapping already exists for typeof {dependentInterfaces.GetType()}");
            }
            
            dependentInterfaces.Add(typeof(TInterface), new(typeof(TImplementation), DependencyLifetime.Singleton));
        }

        public void AddTransient<TInterface, TImplementation>() where TInterface : class
                                                             where TImplementation : class, TInterface        
        {
            if (dependentInterfaces.ContainsKey(typeof(TInterface)))
            {
                throw new MappingExistsException($"Mapping already exists for typeof {dependentInterfaces.GetType()}");
            }
            
            dependentInterfaces.Add(typeof(TInterface), new(typeof(TImplementation), DependencyLifetime.Transient));
        }

        public TInterface GetService<TInterface>() where TInterface : class
        {
            if (dependentInterfaces.TryGetValue(typeof(TInterface), out DependencyImplementation? value))
            {
                // If transient - always return a new instance
                if (value.Scope == DependencyLifetime.Transient)
                {
                    return (TInterface)Activator.CreateInstance(dependentInterfaces[typeof(TInterface)].ImplementationType);
                }

                if (!activeInterfaces.ContainsKey(typeof(TInterface)))
                {
                    activeInterfaces.Add(typeof(TInterface), Activator.CreateInstance(dependentInterfaces[typeof(TInterface)].ImplementationType));
                }

                return (TInterface)activeInterfaces[typeof(TInterface)];
            }
            else
            {
                throw new MappingNotFoundException($"Mapping not found for typeof {typeof(TInterface)}");
            }
        }   
    }
}
