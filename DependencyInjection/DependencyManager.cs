using System.Reflection;
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
            foreach(KeyValuePair<Type, DependencyRequest> buildRequest in requestedInterfaces)
            {
                if (activeInterfaces.ContainsKey(buildRequest.Key))
                    continue;

                // Each implementation will have 1 constructor
                ParameterInfo[] constructorParameters = buildRequest.Value.ImplementationType.GetConstructors()
                                                                                        .First()
                                                                                        .GetParameters();
                // If no constructors, then we can just create the instance
                if (constructorParameters.Length == 0)
                {
                    if (buildRequest.Value.Scope == DependencyLifetime.Singleton)
                    {
                        var singletonInstance = Activator.CreateInstance(buildRequest.Value.ImplementationType) ?? throw new InvalidOperationException("Failed to create singleton instance");

                        // Create and store a single reference to the implementation
                        activeInterfaces.Add(buildRequest.Key, new(() => Convert.ChangeType(singletonInstance, buildRequest.Value.ImplementationType), buildRequest.Value.ImplementationType, buildRequest.Value.Scope));
                    }
                    else if (buildRequest.Value.Scope == DependencyLifetime.Transient)
                    {
                        // Create and store a function to create the implementation
                        activeInterfaces.Add(buildRequest.Key, new(() => Convert.ChangeType(Activator.CreateInstance(buildRequest.Value.ImplementationType), buildRequest.Value.ImplementationType), buildRequest.Value.ImplementationType, buildRequest.Value.Scope));
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

        private void RecursivelyBuildReference(KeyValuePair<Type, DependencyRequest> buildRequest)
        {

        }

        public TInterface GetService<TInterface>() where TInterface : class
        {
            if (!isBuilt)
                throw new InvalidOperationException("Cannot request a service prior to the container being built");

            if (requestedInterfaces.TryGetValue(typeof(TInterface), out DependencyRequest? value))
            {
                // If singleton - always return same instance
                //if (value.Scope == DependencyLifetime.Singleton)
                //{
                //    return (TInterface)activeInterfaces[typeof(TInterface)].ContainedObject;
                //}

                return (TInterface)((Func<object>)activeInterfaces[typeof(TInterface)].ContainedObject)();
            }
            else
            {
                throw new MappingNotFoundException($"Mapping not found for typeof {typeof(TInterface)}");
            }
        }
    }
}
