using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIUnitTests
{
    public interface TestInterface
    {
        string ReturnString();
    }

    public interface TestInterface2
    {
        string ReturnString();
    }   

    public interface TestInterface3DependingOnTwo
    {
        string ReturnChildString();
    }
    
    public interface UnusedInterface
    {
        string ReturnString();
    }

    public class TestClass1 : TestInterface
    {
        public string ReturnString()
        {
            return "Hello World!";
        }
    }

    public class TestClass2 : TestInterface2
    {
        public string ReturnString()
        {
            return "Goodbye World!";
        }
    }

    public class TestClass1WithMultipleConstructors : TestInterface
    {
        private readonly string? testString;

        public TestClass1WithMultipleConstructors()
        {
        }

        public TestClass1WithMultipleConstructors(string testString)
        {
            this.testString = testString;
        }

        public string ReturnString()
        {
            return testString ?? "Hello World!";
        }
    }

    public class TestClass3DependingOnTwo : TestInterface3DependingOnTwo
    {
        private readonly TestInterface2 testInterfaceTwo;

        public TestClass3DependingOnTwo(TestInterface2 testInterfaceTwo)
        {
            this.testInterfaceTwo = testInterfaceTwo;
        }

        public string ReturnChildString()
        {
            return testInterfaceTwo.ReturnString();
        }
    }


    public class UnusedTestClass : UnusedInterface
    {
        public string ReturnString()
        {
            return "This class is not used!";
        }
    }

    public class TestClassRequiring1 : TestInterface
    {
        private TestInterface _testInterface;

        public TestClassRequiring1(TestInterface testInterface)
        {
            _testInterface = testInterface;
        }

        public string ReturnString()
        {
            return _testInterface.ReturnString();
        }
    }

    public class TestClassRequiring2 : TestInterface2
    {
        private TestInterface2 _testInterface;

        public TestClassRequiring2(TestInterface2 testInterface)
        {
            _testInterface = testInterface;
        }

        public string ReturnString()
        {
            return _testInterface.ReturnString();
        }
    }
}
