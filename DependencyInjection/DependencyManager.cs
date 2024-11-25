using System.Reflection;
using System.Runtime.CompilerServices;
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

        public TInterface GetService<TInterface>() where TInterface : class
        {
            if (!isBuilt)
                throw new InvalidOperationException("Cannot request a service prior to the container being built");

            if (!requestedInterfaces.TryGetValue(typeof(TInterface), out DependencyRequest? value))
                throw new MappingNotFoundException($"Mapping not found for typeof {typeof(TInterface)}");

            return (TInterface)((Func<object>)activeInterfaces[typeof(TInterface)].ContainedObject)();
        }

        public void Build()
        {
            foreach(KeyValuePair<Type, DependencyRequest> buildRequest in requestedInterfaces)
            {
                RecursivelyBuildDependencies(buildRequest.Key, buildRequest.Value);
            }

            isBuilt = true;
        }

        private void RecursivelyBuildDependencies(Type typeofInterface, DependencyRequest buildRequest)
        {
            if (activeInterfaces.ContainsKey(typeofInterface))
                return;

            // Each implementation will have 1 constructor
            ParameterInfo[] constructorParameters = GetParametersFromType(buildRequest.ImplementationType);

            // If no constructors, then we can just create the instance
            if (constructorParameters.Length == 0)
            {
                CreateNewObjectFromInterface(typeofInterface, buildRequest);

                return;
            }

            foreach (var param in constructorParameters)
            {
                if (!requestedInterfaces.TryGetValue(param.ParameterType, out DependencyRequest? depReq))
                {
                    throw new MappingNotFoundException($"Mapping not found for typeof {param.ParameterType}");
                }

                // Need to deal with the fact that some of the dependent interfaces may not yet be created
                if (!activeInterfaces.ContainsKey(param.ParameterType))
                {
                    var dep = new DependencyRequest(depReq.ImplementationType, depReq.Scope);
                    RecursivelyBuildDependencies(param.ParameterType, dep);
                }
            }

            CreateNewObjectFromInterface(typeofInterface, buildRequest, constructorParameters);
        }

        private Func<object> CreateNewObjectFromInterface(Type typeofInterface, DependencyRequest buildRequest, ParameterInfo[]? parameterInfo = null)
        {
            var paramTypes = parameterInfo?.Select(p => p.ParameterType);
            object[]? constructorsArgs = paramTypes?.Select(par => CreateNewObjectFromInterface(par, requestedInterfaces[par],
                                                                  GetParametersFromType(par))())
                                                    .ToArray();

            if (activeInterfaces.TryGetValue(typeofInterface, out DependencyImplementation? existing))
            {
                return existing.ContainedObject;
            }

            if (buildRequest.Scope == DependencyLifetime.Singleton)
            {
                var singletonInstance = Activator.CreateInstance(buildRequest.ImplementationType, constructorsArgs) ?? throw new InvalidOperationException("Failed to create singleton instance");

                // Create and store a single reference to the implementation
                activeInterfaces.Add(typeofInterface, new(() => singletonInstance, buildRequest.ImplementationType, buildRequest.Scope));
            }
            else if (buildRequest.Scope == DependencyLifetime.Transient)
            {
                // Create and store a function to create the implementation
                activeInterfaces.Add(typeofInterface, new(() => Activator.CreateInstance(buildRequest.ImplementationType, constructorsArgs), buildRequest.ImplementationType, buildRequest.Scope));
            }
            else
            {
                throw new InvalidOperationException("Unknown lifetime scope");
            }

            return activeInterfaces[typeofInterface].ContainedObject;
        }

        private static ParameterInfo[] GetParametersFromType(Type t) => t.GetConstructors().FirstOrDefault()?.GetParameters() ?? [];
    }
}
