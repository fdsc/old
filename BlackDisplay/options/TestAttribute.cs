using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests
{
    [AttributeUsage(AttributeTargets.Method)]
    public class TestAttribute: Attribute
    {
    }

    public class TestMethodResult
    {
        public enum generalTestResult {skipped = -1, errorFound = 1, success = 0, testError = 3};

        public readonly generalTestResult result;

        public string message = null;
        public string errorInfo = null;

        public TestMethodResult(generalTestResult result)
        {
            this.result = result;
        }
    }
}
