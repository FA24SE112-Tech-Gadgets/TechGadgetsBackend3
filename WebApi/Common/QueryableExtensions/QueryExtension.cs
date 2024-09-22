using System.Linq.Expressions;

namespace WebApi.Common.QueryableExtensions;

public static class QueryableExtension
{
    public static IOrderedQueryable<T> OrderByColumn<T>(
            this IQueryable<T> query,
            Expression<Func<T, object>> sortExpression,
            string? sortOrder) where T : class
    {
        return sortOrder?.ToLower() == "desc"
            ? query.OrderByDescending(sortExpression)
            : query.OrderBy(sortExpression);
    }
}
