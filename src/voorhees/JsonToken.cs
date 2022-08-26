namespace Voorhees {
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