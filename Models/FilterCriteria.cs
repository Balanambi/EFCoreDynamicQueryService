// Filter criteria class
public class FilterCriteria
{
    public string PropertyName { get; set; }
    public object Value { get; set; }
    public FilterOperation Operation { get; set; } = FilterOperation.Equal;
    public Type TargetType { get; set; } // For type conversion
}

// Filter operation enum
public enum FilterOperation
{
    Equal,
    NotEqual,
    Contains,
    StartsWith,
    EndsWith,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual
}
