using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Test.Framework
{
    public record TestCaseInfo
    {
        public string ClassName { get; set; }
        public string MethodName { get; set; }
        public Exception Exception { get; set; }

        public TestCaseInfo(string className, string methodName, Exception exception)
        {
            ClassName = className;
            MethodName = methodName;
            Exception = exception;
        }
    }
}
