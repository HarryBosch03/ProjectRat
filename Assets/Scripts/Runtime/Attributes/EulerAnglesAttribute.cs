using System;
using NUnit.Framework;

namespace Runtime.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EulerAnglesAttribute : PropertyAttribute
    {
        
    }
}