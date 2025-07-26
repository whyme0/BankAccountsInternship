namespace Api.Exceptions;

public class NotFoundException(string? message = null) : Exception(message);