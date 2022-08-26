namespace Voorhees {
    /// JSON data type
    public enum JsonType : byte {
        Unspecified,

        Null,

        Object,
        Array,

        String,
        Boolean,

        // Json doesn't distinguish between number types, but it's often
        // useful to represent them as either ints or floats
        Int,
        Float
    }
}