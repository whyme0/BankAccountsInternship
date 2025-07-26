namespace Api.Exceptions
{
    public class NotFoundException(string? message = default) : Exception(message)
    {
    }
}