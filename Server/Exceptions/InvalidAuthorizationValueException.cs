using System.Runtime.Serialization;

namespace Server.Exceptions;
internal class InvalidAuthorizationValueException : Exception {
    public InvalidAuthorizationValueException() {
    }

    public InvalidAuthorizationValueException(string? message) : base(message) {
    }

    public InvalidAuthorizationValueException(string? message, Exception? innerException) : base(message, innerException) {
    }
}
