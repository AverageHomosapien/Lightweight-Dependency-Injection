namespace DependencyInjection
{
    public class DependencyManager
    {
        private Dictionary<Type, DependencyImplementation> dependentInterfaces = new();
        private Dictionary<Type, Type> activeInterfaces = new();

        public void AddScoped<TInterface, TImplementation>() where TInterface : class
                                                             where TImplementation : class, TInterface
                                                          
        {
            if (dependentInterfaces.ContainsKey(typeof(TInterface)))
            {
                throw new MappingExistsException($"Mapping already exists for typeof {dependentInterfaces.GetType()}");
            }
            
            dependentInterfaces.Add(typeof(TInterface), new(typeof(TImplementation), DependencyScope.Scoped));
        }

        public void AddSingleton<TInterface, TImplementation>() where TInterface : class
                                                             where TImplementation : class, TInterface        
        {
            if (dependentInterfaces.ContainsKey(typeof(TInterface)))
            {
                throw new MappingExistsException($"Mapping already exists for typeof {dependentInterfaces.GetType()}");
            }
            
            dependentInterfaces.Add(typeof(TInterface), new(typeof(TImplementation), DependencyScope.Singleton));
        }

        public void AddTransient<TInterface, TImplementation>() where TInterface : class
                                                             where TImplementation : class, TInterface        
        {
            if (dependentInterfaces.ContainsKey(typeof(TInterface)))
            {
                throw new MappingExistsException($"Mapping already exists for typeof {dependentInterfaces.GetType()}");
            }
            
            dependentInterfaces.Add(typeof(TInterface), new(typeof(TImplementation), DependencyScope.Transient));
        }

        public TInterface GetService<TInterface>() where TInterface : class
        {
            if (dependentInterfaces.ContainsKey(typeof(TInterface)))
            {
                activeInterfaces.Add(typeof(TInterface), dependentInterfaces[typeof(TInterface)].ImplementationType);
                return (TInterface)Activator.CreateInstance(dependentInterfaces[typeof(TInterface)].ImplementationType);
            }
            else
            {
                throw new MappingNotFoundException($"Mapping not found for typeof {typeof(TInterface)}");
            }
        }   
    }
}
