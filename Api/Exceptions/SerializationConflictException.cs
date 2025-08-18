namespace Api.Exceptions;

public class SerializationConflictException(string message) : ConflictException(message);