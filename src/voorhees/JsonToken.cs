namespace Voorhees {
    /// The type of a token in a json document
    public enum JsonToken {
        None,

        ArrayStart,
        ArrayEnd,

        ObjectStart,
        KeyValueSeparator, // : 
        ObjectEnd,

        Separator, // ,

        String,
        Number,
        True,
        False,
        Null,

        EOF
    }
}