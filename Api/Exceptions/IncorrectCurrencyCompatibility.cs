namespace Api.Exceptions
{
    public class IncorrectCurrencyCompatibility(string currency1, string currency2) : Exception($"Currency incompatibility ({currency1} and {currency2})")
    {

    }
}
