using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LockersUnphu.Data;

internal static class SqlErrors
{
    public static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };
}
