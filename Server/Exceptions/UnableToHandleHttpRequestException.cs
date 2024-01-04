using System.Runtime.Serialization;

namespace Server.Exceptions;
internal class UnableToHandleHttpRequestException : Exception {
    public UnableToHandleHttpRequestException() {
    }

    public UnableToHandleHttpRequestException(string? message) : base(message) {
    }

    public UnableToHandleHttpRequestException(string? message, Exception? innerException) : base(message, innerException) {
    }
}
