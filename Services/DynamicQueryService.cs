// Main service implementation
public class DynamicQueryService : IDynamicQueryService
{
    public IQueryable<T> ApplyDynamicFilter<T>(IQueryable<T> query, string propertyName, object value, FilterOperation operation = FilterOperation.Equal) where T : class
    {
        var criteria = new FilterCriteria
        {
            PropertyName = propertyName,
            Value = value,
            Operation = operation
        };

        return ApplyMultipleFilters(query, new List<FilterCriteria> { criteria });
    }

    public IQueryable<T> ApplyMultipleFilters<T>(IQueryable<T> query, List<FilterCriteria> filters) where T : class
    {
        if (filters == null || !filters.Any())
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression combinedExpression = null;

        foreach (var filter in filters)
        {
            var filterExpression = BuildFilterExpression<T>(parameter, filter);
            
            if (combinedExpression == null)
                combinedExpression = filterExpression;
            else
                combinedExpression = Expression.AndAlso(combinedExpression, filterExpression);
        }

        if (combinedExpression != null)
        {
            var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
            query = query.Where(lambda);
        }

        return query;
    }

    private Expression BuildFilterExpression<T>(ParameterExpression parameter, FilterCriteria filter)
    {
        var property = GetPropertyExpression(parameter, filter.PropertyName);
        var propertyType = GetPropertyType<T>(filter.PropertyName);
        
        // Handle type conversion - this is the key part for your scenario
        var convertedProperty = ConvertPropertyIfNeeded(property, propertyType, filter.TargetType ?? filter.Value?.GetType());
        var convertedValue = ConvertValueExpression(filter.Value, filter.TargetType ?? propertyType);

        return BuildComparisonExpression(convertedProperty, convertedValue, filter.Operation);
    }

    private Expression GetPropertyExpression(ParameterExpression parameter, string propertyName)
    {
        Expression property = parameter;
        
        // Handle nested properties (e.g., "User.Name")
        foreach (var member in propertyName.Split('.'))
        {
            property = Expression.PropertyOrField(property, member);
        }
        
        return property;
    }

    private Type GetPropertyType<T>(string propertyName)
    {
        Type currentType = typeof(T);
        
        foreach (var member in propertyName.Split('.'))
        {
            var propertyInfo = currentType.GetProperty(member);
            if (propertyInfo != null)
            {
                currentType = propertyInfo.PropertyType;
            }
            else
            {
                var fieldInfo = currentType.GetField(member);
                currentType = fieldInfo?.FieldType ?? currentType;
            }
        }
        
        return currentType;
    }

    private Expression ConvertPropertyIfNeeded(Expression property, Type sourceType, Type targetType)
    {
        if (sourceType == targetType)
            return property;

        // Handle nullable types
        var underlyingSourceType = Nullable.GetUnderlyingType(sourceType) ?? sourceType;
        var underlyingTargetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // Convert int to string (your main scenario)
        if (underlyingSourceType == typeof(int) && underlyingTargetType == typeof(string))
        {
            // Use EF.Functions.Convert for database-level conversion
            var efFunctionsType = typeof(EF);
            var functionsProperty = efFunctionsType.GetProperty("Functions");
            var functionsInstance = Expression.Property(null, functionsProperty);
            
            // Get the Convert method
            var convertMethod = typeof(DbFunctions).GetMethod("Convert", 
                new[] { typeof(string), typeof(object) });
            
            if (convertMethod != null)
            {
                var convertCall = Expression.Call(functionsInstance, convertMethod,
                    Expression.Constant("NVARCHAR(50)"), // Adjust length as needed
                    Expression.Convert(property, typeof(object)));
                
                return convertCall;
            }
        }

        // Convert string to int
        if (underlyingSourceType == typeof(string) && underlyingTargetType == typeof(int))
        {
            var convertMethod = typeof(Convert).GetMethod("ToInt32", new[] { typeof(string) });
            return Expression.Call(convertMethod, property);
        }

        // Handle other common conversions
        if (underlyingTargetType == typeof(string))
        {
            var toStringMethod = underlyingSourceType.GetMethod("ToString", Type.EmptyTypes);
            if (toStringMethod != null)
            {
                return Expression.Call(property, toStringMethod);
            }
        }

        // Default: try direct conversion
        return Expression.Convert(property, targetType);
    }

    private Expression ConvertValueExpression(object value, Type targetType)
    {
        if (value == null)
            return Expression.Constant(null, targetType);

        if (value.GetType() == targetType)
            return Expression.Constant(value, targetType);

        try
        {
            var convertedValue = Convert.ChangeType(value, targetType);
            return Expression.Constant(convertedValue, targetType);
        }
        catch
        {
            // If conversion fails, return as-is
            return Expression.Constant(value);
        }
    }

    private Expression BuildComparisonExpression(Expression left, Expression right, FilterOperation operation)
    {
        switch (operation)
        {
            case FilterOperation.Equal:
                return Expression.Equal(left, right);
            
            case FilterOperation.NotEqual:
                return Expression.NotEqual(left, right);
            
            case FilterOperation.Contains:
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                return Expression.Call(left, containsMethod, right);
            
            case FilterOperation.StartsWith:
                var startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
                return Expression.Call(left, startsWithMethod, right);
            
            case FilterOperation.EndsWith:
                var endsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
                return Expression.Call(left, endsWithMethod, right);
            
            case FilterOperation.GreaterThan:
                return Expression.GreaterThan(left, right);
            
            case FilterOperation.LessThan:
                return Expression.LessThan(left, right);
            
            case FilterOperation.GreaterThanOrEqual:
                return Expression.GreaterThanOrEqual(left, right);
            
            case FilterOperation.LessThanOrEqual:
                return Expression.LessThanOrEqual(left, right);
            
            default:
                return Expression.Equal(left, right);
        }
    }
}
