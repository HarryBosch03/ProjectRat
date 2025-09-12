using System;
using NUnit.Framework.Constraints;
using UnityEngine;

namespace Runtime.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class KmpHAttribute : PropertyAttribute
    {
        
    }
}