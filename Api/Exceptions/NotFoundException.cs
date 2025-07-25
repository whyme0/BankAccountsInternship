namespace Api.Exceptions
{
    public class NotFoundException(string propertyValue) : Exception($"Entity with property '{propertyValue}' not found")
    {
    }
}
