using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

// Service interface
public interface IDynamicQueryService
{
    IQueryable<T> ApplyDynamicFilter<T>(IQueryable<T> query, string propertyName, object value, FilterOperation operation = FilterOperation.Equal) where T : class;
    IQueryable<T> ApplyMultipleFilters<T>(IQueryable<T> query, List<FilterCriteria> filters) where T : class;
}