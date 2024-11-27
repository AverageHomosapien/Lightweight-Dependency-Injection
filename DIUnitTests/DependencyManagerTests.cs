using Lightweight.Dependency.Injection;
using Lightweight.Dependency.Injection.Exceptions;

namespace DIUnitTests
{
    public class DependencyManagerTests
    {
        [Fact]
        public void WhenDependencyAdded_Expect_ManagerCanResolveDependencies()
        {
            DependencyManager dm = new();
            dm.AddSingleton<TestInterface, TestClass1>();
            dm.Build();

            var service = dm.GetService<TestInterface>();
            Assert.Equal("Hello World!", service.ReturnString());
        }

        [Fact]
        public void WhenAddingDuplicateServices_Expect_ThrowsException()
        {
            DependencyManager dm = new();
            dm.AddTransient<TestInterface, TestClass1>();

            Assert.Throws<MappingExistsException>(() => dm.AddSingleton<TestInterface, TestClass1>());
        }

        [Fact]
        public void WhenAddingDuplicateServicesOfDifferentTypes_Expect_ThrowsException()
        {
            DependencyManager dm = new();
            dm.AddSingleton<TestInterface, TestClass1>();

            Assert.Throws<MappingExistsException>(() => dm.AddSingleton<TestInterface, TestClass1>());
            Assert.Throws<MappingExistsException>(() => dm.AddTransient<TestInterface, TestClass1>());
        }

        [Fact]
        public void WhenCallingInstanceBeforeDependencyManagerBuilt_Expect_ThrowsException()
        {
            DependencyManager dm = new();
            dm.AddSingleton<TestInterface, TestClass1>();

            Assert.Throws<InvalidOperationException>(() => dm.GetService<TestInterface>());
        }

        [Fact]
        public void WhenCallingInstanceWithMultipleConstructors_Expect_ThrowsException()
        {
            DependencyManager dm = new();
            Assert.Throws<MappingMutipleConstructorException>(() => dm.AddSingleton<TestInterface, TestClass1WithMultipleConstructors>());
        }

        [Fact]
        public void WhenCallingServicesThatHaventBeenRegistered_Expect_ThrowsException()
        {
            DependencyManager dm = new();
            dm.Build();

            Assert.Throws<MappingNotFoundException>(() => dm.GetService<TestInterface>());
        }

        [Fact]
        public void WhenCallingSingletonExpect_SameInstanceReturned()
        {
            DependencyManager dm = new();
            dm.AddSingleton<TestInterface, TestClass1>();
            dm.Build();

            var service1 = dm.GetService<TestInterface>();
            var service2 = dm.GetService<TestInterface>();

            Assert.Same(service1, service2);
        }

        [Fact]
        public void WhenCallingTransientExpect_SameDifferentInstanceReturned()
        {
            DependencyManager dm = new();
            dm.AddTransient<TestInterface, TestClass1>();
            dm.Build();

            var service1 = dm.GetService<TestInterface>();
            var service2 = dm.GetService<TestInterface>();

            Assert.NotSame(service1, service2);
        }

        [Fact]
        public void WhenCallingClassThatInheritsInterfaceInConstructor_ExpectResolvedCorrectly()
        {
            DependencyManager dm = new();
            dm.AddSingleton<TestInterface, TestClass1>();
            dm.AddSingleton<TestInterface2, TestClass2>();
            dm.AddSingleton<TestInterface3DependingOnTwo, TestClass3DependingOnTwo>();
            dm.Build();

            var service = dm.GetService<TestInterface3DependingOnTwo>();
            Assert.Equal("Goodbye World!", service.ReturnChildString());
        }

        [Fact]
        public void WhenAddingDependenciesInTheWrongOrder_ExpectResolvedCorrectly()
        {
            DependencyManager dm = new();
            dm.AddSingleton<TestInterface3DependingOnTwo, TestClass3DependingOnTwo>();
            dm.AddSingleton<TestInterface2, TestClass2>();
            dm.Build();

            var service = dm.GetService<TestInterface3DependingOnTwo>();
            Assert.Equal("Goodbye World!", service.ReturnChildString());
        }
    }
}