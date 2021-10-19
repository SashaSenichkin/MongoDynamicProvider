using System;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Driver;

namespace Stp.Tools.MongoDB.Expressions
{
    /// <summary>
    /// Order direction for MongoDB collection
    /// </summary>
    public enum OrderDirection
    {
        Asc,
        Desc
    }

    /// <summary>
    /// MongoDB expressions
    /// </summary>
    public static class MongoDbExpressions
    {
        /// <summary>
        /// Sorting DB collection by property name
        /// </summary>
        /// <param name="source">source collection</param>
        /// <param name="propertyName">Property name (better use nameof(...))</param>
        /// <param name="direction">Asc or Desc</param>
        public static IOrderedFindFluent<TDocument, TProjection> SortByPropertyName<TDocument, TProjection>(
            this IFindFluent<TDocument, TProjection> source, string propertyName, OrderDirection direction)
        {
            var mongodbMethodName = direction == OrderDirection.Asc ? nameof(IFindFluentExtensions.SortBy) : nameof(IFindFluentExtensions.SortByDescending);
            return GetOrderedFromSource(source, propertyName, mongodbMethodName);
        }

        /// <summary>
        /// Sorting already sorted DB collection by property name
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TProjection"></typeparam>
        /// <param name="source"></param>
        /// <param name="propertyName">Property name (better use nameof(...))</param>
        /// <param name="direction">Asc or Desc</param>
        public static IOrderedFindFluent<TDocument, TProjection> ThenSortByPropertyName<TDocument, TProjection>(
            this IOrderedFindFluent<TDocument, TProjection> source, string propertyName, OrderDirection direction)
        {
            var mongodbMethodName = direction == OrderDirection.Asc ? nameof(IFindFluentExtensions.ThenBy) : nameof(IFindFluentExtensions.ThenByDescending);
            return GetOrderedFromSource(source, propertyName, mongodbMethodName);
        }

        private static IOrderedFindFluent<TDocument, TProjection> GetOrderedFromSource<TDocument, TProjection>(
            IFindFluent<TDocument, TProjection> source, string propertyName, string methodName)
        {
            var entityType = typeof(TDocument);
            var entityPropertyInfo = entityType.GetProperties()
                                               .FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));
            if (entityPropertyInfo is null)
            {
                throw new Exception($"Property \"{propertyName}\" not found");
            }

            var method = typeof(IFindFluentExtensions).GetMethods().FirstOrDefault(m => m.Name == methodName);

            if (method is null)
            {
                throw new Exception($"Method \"{methodName}\" not found");
            }

            var expArgument = Expression.Parameter(entityType, "x");
            var property = Expression.Property(expArgument, entityPropertyInfo);
            var lambdaExp = Expression.Lambda(Expression.Convert(property, typeof(object)), expArgument);

            var genericMethod = method.MakeGenericMethod(entityType, typeof(TProjection));
            var result = genericMethod.Invoke(null, new object[] { source, lambdaExp }) as IOrderedFindFluent<TDocument, TProjection>;

            return result;
        }
    }
}
