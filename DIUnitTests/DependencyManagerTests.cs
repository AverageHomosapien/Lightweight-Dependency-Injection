using DependencyInjection;

namespace DIUnitTests
{
    public class DependencyManagerTests
    {
        [Fact]
        public void WhenDependencyAdded_Expect_ManagerCanResolveDependencies()
        {
            DependencyManager dm = new();
            dm.AddScoped<TestInterface, TestClass1>();

            var service = dm.GetService<TestInterface>();
            Assert.Equal("Hello World!", service.ReturnString());
            // spin up different instances and check if they are the same
        }

        [Fact]
        public void WhenAddingDuplicateServices_Expect_ThrowsException()
        {
            DependencyManager dm = new();
            dm.AddScoped<TestInterface, TestClass1>();

            Assert.Throws<MappingExistsException>(() => dm.AddScoped<TestInterface, TestClass1>());
        }

        [Fact]
        public void WhenAddingDuplicateServicesOfDifferentTypes_Expect_ThrowsException()
        {
            DependencyManager dm = new();
            dm.AddScoped<TestInterface, TestClass1>();

            Assert.Throws<MappingExistsException>(() => dm.AddSingleton<TestInterface, TestClass1>());
            Assert.Throws<MappingExistsException>(() => dm.AddTransient<TestInterface, TestClass1>());
        }

        [Fact]
        public void WhenCallingServicesThatHaventBeenRegistered_Expect_ThrowsException()
        {
            DependencyManager dm = new();

            Assert.Throws<MappingNotFoundException>(() => dm.GetService<TestInterface>());
        }
    }
}