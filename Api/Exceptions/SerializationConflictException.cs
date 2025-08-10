namespace Api.Exceptions;

public class SerializationConflictException(string message) : Exception(message);