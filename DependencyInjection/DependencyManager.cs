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
        private Dictionary<Type, DependencyRequest> userDefinedDependencyRequests = new();

        /// <summary>
        /// Holds a set of active instances of dependency interfaces
        /// </summary>
        private Dictionary<Type, DependencyImplementation> activeDependencies = new();

        /// <summary>
        /// Used for adding a Singleton instance of an Interface -> Implementation Mapping
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="MappingMutipleConstructorException">Thrown for classes with multiple constructors</exception>
        /// <exception cref="MappingExistsException"></exception>
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

            if (userDefinedDependencyRequests.ContainsKey(typeof(TInterface)))
            {
                throw new MappingExistsException($"Mapping already exists for typeof {userDefinedDependencyRequests.GetType()}");
            }

            userDefinedDependencyRequests.Add(typeof(TInterface), new(typeof(TImplementation), DependencyLifetime.Singleton));
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

            if (userDefinedDependencyRequests.ContainsKey(typeof(TInterface)))
            {
                throw new MappingExistsException($"Mapping already exists for typeof {userDefinedDependencyRequests.GetType()}");
            }

            userDefinedDependencyRequests.Add(typeof(TInterface), new(typeof(TImplementation), DependencyLifetime.Transient));
        }

        public TInterface GetService<TInterface>() where TInterface : class
        {
            if (!isBuilt)
                throw new InvalidOperationException("Cannot request a service prior to the container being built");

            if (!userDefinedDependencyRequests.TryGetValue(typeof(TInterface), out DependencyRequest? value))
                throw new MappingNotFoundException($"Mapping not found for typeof {typeof(TInterface)}");

            return (TInterface)activeDependencies[typeof(TInterface)].ContainedObject();
        }

        public void Build()
        {
            foreach(KeyValuePair<Type, DependencyRequest> buildRequest in userDefinedDependencyRequests)
            {
                BuildDependency(buildRequest.Key, buildRequest.Value);
            }

            isBuilt = true;
        }

        /// <summary>
        /// Builds a Dependency for a requested scope
        /// </summary>
        /// <param name="typeofInterface"></param>
        /// <param name="buildRequest"></param>
        /// <exception cref="MappingNotFoundException"></exception>
        private void BuildDependency(Type typeofInterface, DependencyRequest buildRequest)
        {
            // Since we sometimes need to build dependencies out of order (e.g. if a user has requested to build class B that depends on class A, before requesting to build class A)
            if (activeDependencies.ContainsKey(typeofInterface))
                return;

            // Each implementation will have 1 constructor
            IEnumerable<Type> constructorParameterTypes = GetTypedConstructorParamsOfType(buildRequest.ImplementationType).ToList();

            // If no constructors, then we can just create the instance
            if (!constructorParameterTypes.Any())
            {
                CreateInjectableDependency(typeofInterface, buildRequest);
                return;
            }

            // Building up our tree of dependencies for each object (to ensure they exist when we try and create the object that relies on them)
            foreach (var paramType in constructorParameterTypes)
            {
                if (!userDefinedDependencyRequests.TryGetValue(paramType, out DependencyRequest? depReq))
                {
                    throw new MappingNotFoundException($"Mapping not found for typeof {paramType}");
                }

                // Need to deal with the fact that some of the dependent interfaces may not yet be created
                if (!activeDependencies.ContainsKey(paramType))
                {
                    var dep = new DependencyRequest(depReq.ImplementationType, depReq.Scope);
                    BuildDependency(paramType, dep);
                }
            }

            CreateInjectableDependency(typeofInterface, buildRequest, constructorParameterTypes);
        }

        private Func<object> CreateInjectableDependency(Type typeofInterface, DependencyRequest buildRequest, IEnumerable<Type>? constructorTypes = null)
        {
            // Recursively creates all the dependencies needed for an object constructor and returns the methods to create the object
            IEnumerable<Func<object>>? constructorGenerators = constructorTypes?.Select(par => CreateInjectableDependency(par, userDefinedDependencyRequests[par], GetTypedConstructorParamsOfType(par)));
            
            // Execute all the methods to create the objects
            object[]? constructorArgs = constructorGenerators?.Select(constructor => constructor())?
                                                              .ToArray();

            if (activeDependencies.TryGetValue(typeofInterface, out DependencyImplementation? existing))
            {
                return existing.ContainedObject;
            }

            if (buildRequest.Scope == DependencyLifetime.Singleton)
            {
                var singletonInstance = Activator.CreateInstance(buildRequest.ImplementationType, constructorArgs) ?? throw new InvalidOperationException("Failed to create singleton instance");

                // Create and store a single reference to the implementation
                activeDependencies.Add(typeofInterface, new(() => singletonInstance, buildRequest.ImplementationType, buildRequest.Scope));
            }
            else if (buildRequest.Scope == DependencyLifetime.Transient)
            {
                // Create and store a function to create the implementation
                activeDependencies.Add(typeofInterface, new(() => Activator.CreateInstance(buildRequest.ImplementationType, constructorArgs), buildRequest.ImplementationType, buildRequest.Scope));
            }
            else
            {
                throw new InvalidOperationException("Unknown lifetime scope");
            }

            return activeDependencies[typeofInterface].ContainedObject;
        }

        private static IEnumerable<Type> GetTypedConstructorParamsOfType(Type t) => t.GetConstructors().FirstOrDefault()?.GetParameters().Select(param => param.ParameterType) ?? [];
    }
}
