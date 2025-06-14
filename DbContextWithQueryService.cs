public class ExampleUsage
{
    private readonly DbContext _context;
    private readonly IDynamicQueryService _queryService;

    public ExampleUsage(DbContext context, IDynamicQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    // Example: Query where int property needs to be compared as string
    public async Task<List<MyEntity>> GetEntitiesWithStringComparison(int searchValue)
    {
        var query = _context.Set<MyEntity>().AsQueryable();
        
        // Method 1: Using the service directly
        var filteredQuery = _queryService.ApplyDynamicFilter(query, "IntColumn", searchValue.ToString(), FilterOperation.Equal);
        
        return await filteredQuery.ToListAsync();
    }

    // Example: Multiple filters with different types
    public async Task<List<MyEntity>> GetEntitiesWithMultipleFilters(int intValue, string nameFilter)
    {
        var filters = new List<FilterCriteria>
        {
            new FilterCriteria 
            { 
                PropertyName = "IntColumn", 
                Value = intValue.ToString(), 
                Operation = FilterOperation.Equal,
                TargetType = typeof(string)
            },
            new FilterCriteria 
            { 
                PropertyName = "Name", 
                Value = nameFilter, 
                Operation = FilterOperation.Contains 
            }
        };

        var query = _context.Set<MyEntity>().AsQueryable();
        var filteredQuery = _queryService.ApplyMultipleFilters(query, filters);
        
        return await filteredQuery.ToListAsync();
    }

    // Example: Using extension methods
    public async Task<List<MyEntity>> GetEntitiesUsingExtensions(int searchValue)
    {
        var result = await _context.Set<MyEntity>()
            .WhereDynamic("IntColumn", searchValue.ToString(), FilterOperation.Equal)
            .ToListAsync();
            
        return result;
    }
}