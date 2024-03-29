﻿namespace Voorhees {
    /// The type of a json value
    public enum JsonValueType : byte {
        Unspecified,

        Null,

        Object,
        Array,

        String,
        Boolean,

        // Json doesn't distinguish between number types, but it's often
        // useful to represent them as either ints or doubles to ease parsing of json data
        Int,
        Double
    }
}
