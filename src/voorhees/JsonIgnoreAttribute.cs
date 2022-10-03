using System;

namespace Voorhees {
    /// Indicates that the field or property should be ignored
    /// when mapping to and from JSON data.
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class JsonIgnoreAttribute : Attribute {
    }
}
