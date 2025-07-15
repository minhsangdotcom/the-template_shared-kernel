using Microsoft.EntityFrameworkCore;

namespace SharedKernel.Extensions.QueryExtensions;

public static class PostgresDbFunctionExtensions
{
    [DbFunction("unaccent", IsBuiltIn = true)]
    public static string Unaccent(string input) => throw new NotSupportedException();
}
