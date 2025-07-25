namespace Api.Exceptions
{
    public class NotEnoughBalanceException(decimal currBalance, string propertyValue) : Exception($"Not enough ({currBalance}) on account ({propertyValue})")
    {
    }
}
