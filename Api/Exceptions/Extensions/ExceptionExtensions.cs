using Npgsql;

namespace Api.Exceptions.Extensions;

public static class ExceptionExtensions
{
    public static bool TryFindPostgresException(Exception? ex, out PostgresException? pg)
    {
        while (ex != null)
        {
            if (ex is PostgresException p) { pg = p; return true; }
            ex = ex.InnerException;
        }
        pg = null;
        return false;
    }
}