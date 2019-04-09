using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

public class QueryClientEvaluationWarningDbContext :
    DbContext
{
    #region QueryClientEvaluationWarning

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(
            warnings =>
            {
                warnings.Throw(RelationalEventId.QueryClientEvaluationWarning);
            });
    }

    #endregion
}