using System;

namespace Voorhees {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class JsonIgnoreAttribute : Attribute {
    }
}
