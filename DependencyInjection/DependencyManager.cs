using System.Reflection;
using System.Runtime.Remoting;
using Lightweight.Dependency.Injection.Exceptions;

namespace Lightweight.Dependency.Injection
{
    public class DependencyManager
    {
        private bool isBuilt = false;

        /// <summary>
        /// Holds a set of requested interface -> implementations
        /// </summary>
        private Dictionary<Type, DependencyRequest> requestedInterfaces = new();

        /// <summary>
        /// Holds a set of active instances of dependency interfaces
        /// </summary>
        private Dictionary<Type, DependencyImplementation> activeInterfaces = new();

        public void AddSingleton<TInterface, TImplementation>() where TInterface : class
                                                             where TImplementation : class, TInterface
        {
            if (isBuilt)
            {
                throw new InvalidOperationException("Cannot add a new mapping after the container has been built");
            }

            // Don't allow a class with multiple constructors - as we don't know how to resolve which constructor it refers to
            if (typeof(TImplementation).GetConstructors().Length > 1)
            {
                throw new MappingMutipleConstructorException("Cannot add dependencies for a class with multiple constructors");
            }

            if (requestedInterfaces.ContainsKey(typeof(TInterface)))
            {
                throw new MappingExistsException($"Mapping already exists for typeof {requestedInterfaces.GetType()}");
            }

            requestedInterfaces.Add(typeof(TInterface), new(typeof(TImplementation), DependencyLifetime.Singleton));
        }

        public void AddTransient<TInterface, TImplementation>() where TInterface : class
                                                             where TImplementation : class, TInterface
        {
            if (isBuilt)
            {
                throw new InvalidOperationException("Cannot add a new mapping after the container has been built");
            }

            // Don't allow a class with multiple constructors - as we don't know how to resolve which constructor it refers to
            if (typeof(TImplementation).GetConstructors().Length > 1)
            {
                throw new MappingMutipleConstructorException("Cannot add dependencies for a class with multiple constructors");
            }

            if (requestedInterfaces.ContainsKey(typeof(TInterface)))
            {
                throw new MappingExistsException($"Mapping already exists for typeof {requestedInterfaces.GetType()}");
            }

            requestedInterfaces.Add(typeof(TInterface), new(typeof(TImplementation), DependencyLifetime.Transient));
        }

        public void Build()
        {
            foreach(KeyValuePair<Type, DependencyRequest> request in requestedInterfaces)
            {
                if (activeInterfaces.ContainsKey(request.Key))
                    continue;

                // Each implementation will have 1 constructor
                ParameterInfo[] constructorParameters = request.Value.ImplementationType.GetConstructors()
                                                                                        .First()
                                                                                        .GetParameters();
                // If no constructors, then we can just create the instance
                if (constructorParameters.Length == 0)
                {
                    if (request.Value.Scope == DependencyLifetime.Singleton)
                    {
                        var singletonInstance = Activator.CreateInstance(request.Value.ImplementationType) ?? throw new InvalidOperationException("Failed to create singleton instance");

                        // Create and store a single reference to the implementation
                        activeInterfaces.Add(request.Key, new(singletonInstance, request.Value.ImplementationType, request.Value.Scope));
                    }
                    else if (request.Value.Scope == DependencyLifetime.Transient)
                    {
                        // Create and store a function to create the implementation
                        activeInterfaces.Add(request.Key, new(() => Activator.CreateInstance(request.Value.ImplementationType), request.Value.ImplementationType, request.Value.Scope));
                    }
                    else
                    {
                        throw new InvalidOperationException("Unknown lifetime scope");
                    }

                    continue;
                }

                // Need to deal with the fact that some of the dependent interfaces may not yet be created
                foreach (var param in constructorParameters)
                {
                    var t = param;
                }
            }

            isBuilt = true;
        }

        private void RecursivelyBuildReference()
        {

        }

        public TInterface GetService<TInterface>() where TInterface : class
        {
            if (!isBuilt)
                throw new InvalidOperationException("Cannot request a service prior to the container being built");

            if (requestedInterfaces.TryGetValue(typeof(TInterface), out DependencyRequest? value))
            {
                // If transient - always return a new instance
                if (value.Scope == DependencyLifetime.Transient)
                {
                    return (TInterface)activeInterfaces[typeof(TInterface)].ContainedObject;
                }

                if (!activeInterfaces.ContainsKey(typeof(TInterface)))
                {
                    return (TInterface)activeInterfaces[typeof(TInterface)].ContainedObject;
                }

                return (TInterface)activeInterfaces[typeof(TInterface)].ContainedObject;
            }
            else
            {
                throw new MappingNotFoundException($"Mapping not found for typeof {typeof(TInterface)}");
            }
        }
    }
}
