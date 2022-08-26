using System;

namespace Voorhees {
    /// Thrown when trying to read invalid JSON data.
    public class InvalidJsonException : Exception {
        public InvalidJsonException(string message) : base(message) { }
    }
}