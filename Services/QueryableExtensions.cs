// Extension methods for easier usage
public static class QueryableExtensions
{
    public static IQueryable<T> WhereDynamic<T>(this IQueryable<T> query, string propertyName, object value, FilterOperation operation = FilterOperation.Equal) where T : class
    {
        var service = new DynamicQueryService();
        return service.ApplyDynamicFilter(query, propertyName, value, operation);
    }

    public static IQueryable<T> WhereMultiple<T>(this IQueryable<T> query, List<FilterCriteria> filters) where T : class
    {
        var service = new DynamicQueryService();
        return service.ApplyMultipleFilters(query, filters);
    }
}
