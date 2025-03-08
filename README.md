# Lightweight Dependency Injection

A super lightweight Dependency Injection (DI) tool offering core functionality, including Constructor Injection and a DI Service.

## Features
- Constructor Injection: Allows dependencies to be injected through class constructors, promoting immutability and easier testing.
- DI Service: Provides a centralized service for managing and resolving dependencies across your application.
  
## Installation

The project is available on Nuget as Lightweight.Dependency.Injection (https://www.nuget.org/packages/Lightweight.Dependency.Injection).
It can be added via nuget or using `dotnet add package Lightweight.Dependency.Injection --version 1.0.1`

## Usage
### Registering Services
To register a service with the DI container:
```C#
var dependencyManager = new DependencyManager();
// Register MyService as the implementation for the IMyService interface.
dependencyManager.AddTransient<IMyService, MyService>();
// Register MySingletonService as the singleton implementation for the IMySingletonService interface.
dependencyManager.AddSingleton<IMySingletonService, MySingletonService>();
```

### Resolving Services
To resolve a service the dependency manager needs to be built. Then, services can either be resolved through class constructors, or by calling the dependency manager below as follows:
```C#
dependencyManager.Build();
var myService = dependencyManager.GetService<IMyService>();
```
This retrieves an instance of IMyService from the service provider.

## Contributing
Contributions are welcome! Please fork the repository and submit a pull request with your changes.

## License
This project is licensed under the MIT License. See the LICENSE file for details.

This project is maintained by Calum Hamilton.
