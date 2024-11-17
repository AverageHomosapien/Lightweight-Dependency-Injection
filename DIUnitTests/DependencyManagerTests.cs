using DependencyInjection;

namespace DIUnitTests
{
    public class DependencyManagerTests
    {
        [Fact]
        public void WhenDependencyAdded_Expect_ManagerCanResolveDependencies()
        {
            DependencyManager dm = new();
            dm.AddSingleton<TestInterface, TestClass1>();

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
        public void WhenCallingServicesThatHaventBeenRegistered_Expect_ThrowsException()
        {
            DependencyManager dm = new();

            Assert.Throws<MappingNotFoundException>(() => dm.GetService<TestInterface>());
        }

        [Fact]
        public void WhenCallingSingletonExpect_SameInstanceReturned()
        {
            DependencyManager dm = new();
            dm.AddSingleton<TestInterface, TestClass1>();

            var service1 = dm.GetService<TestInterface>();
            var service2 = dm.GetService<TestInterface>();

            Assert.Same(service1, service2);
        }

        [Fact]
        public void WhenCallingTransientExpect_SameDifferentInstanceReturned()
        {
            DependencyManager dm = new();
            dm.AddTransient<TestInterface, TestClass1>();

            var service1 = dm.GetService<TestInterface>();
            var service2 = dm.GetService<TestInterface>();

            Assert.NotSame(service1, service2);
        }
    }
}