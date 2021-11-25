using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Stp.Tools.MongoDB.Interfaces;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Stp.Tools.MongoDB
{
    /// <summary>
    /// improved work with model and dynamic data.
    /// skip empty fields, ignore extra fields.
    /// Supports Arrays, simple fields, Lists, classes in mongo bsons.
    /// </summary>
    /// <typeparam name="TEntity">Your model type</typeparam>
    public abstract class MongoDataProvider<TEntity> : BaseNoSqlProvider<BsonDocument> where TEntity : class, new()
    {
        protected MongoDataProvider(IMongoDbSettings settings, string nameCollection) : base(settings, nameCollection)
        {
        }
        
        /// <summary>
        /// base method to work. supports mongo filers 
        /// </summary>
        /// <param name="filter">null to get full collection</param>
        /// <returns>list of entities we can parse correctly. Also all errors list, raised while converting, ignore if you don't care</returns>
        protected async Task<(List<TEntity>, List<Exception>)> GetFromBsonDocAsync(FilterDefinition<TEntity> filter = null)
        {
            var filterTypes = filter?.Render(BsonSerializer.SerializerRegistry.GetSerializer<TEntity>(), BsonSerializer.SerializerRegistry) ?? Builders<BsonDocument>.Filter.Empty;

            var foundElems = await base.GetContext().Source.FindAsync(filterTypes).ConfigureAwait(false);
            var foundElemsList = await foundElems.ToListAsync().ConfigureAwait(false);
            return Convert<TEntity>(foundElemsList);
        }
        
        /// <summary>
        /// simple convert of bsonEntities 
        /// </summary>
        /// <param name="source"></param>
        /// <returns>list of entities we, can parse correctly. Also all errors list, raised while, ignore if you don't care</returns>
        protected static (List<TRequest>, List<Exception>) Convert<TRequest>(List<BsonDocument> source) where TRequest : class, new()
        {
            var errors = new List<Exception>();
            var result = source.Select(x => Convert<TRequest>(x, errors)).Where(x => x != null).ToList();
            return (result, errors);
        }

        /// <summary>
        /// reflection parser.
        /// </summary>
        /// <param name="source">document to parse</param>
        /// <param name="errors">add error, if one raised while work. send null if you don't care</param>
        /// <returns>entity of your type or null</returns>
        /// <exception cref="FormatException">some unsupported cases from mongo bson</exception>
        private static TRequest Convert<TRequest>(BsonDocument source, List<Exception> errors) where TRequest : class, new()
        {
            try
            {
                var expandFromBson = BsonSerializer.Deserialize<ExpandoObject>(source);
                var result = CreateAndFill<TRequest>(expandFromBson);
                return result;
            }
            catch (Exception e)
            {
                errors?.Add(e);
                return null;
            }
        }
        
        private static TCustomClass CreateAndFill<TCustomClass>(ExpandoObject source) where TCustomClass : class
        {
            var result = Activator.CreateInstance<TCustomClass>();
            var props = typeof(TCustomClass).GetProperties();
            foreach (var item in source.Where(x => x.Value != null))
            {
                try
                {
                    var property = props.FirstOrDefault(x => IsHaveProperAttribute(x, item.Key));
                    if (property == null)
                    {
                        property = props.FirstOrDefault(x => x.Name.Equals(item.Key, StringComparison.OrdinalIgnoreCase));
                    }

                    if (property != null)
                    {
                        SetValue(property, item, result);
                    }
                }
                catch (Exception e)
                {
                    throw new FormatException($"cant convert {item} to entity {typeof(TCustomClass)}", e);
                }
            }
            
            return result;
        }

        private static void SetValue<TCustomClass>(PropertyInfo property, KeyValuePair<string,object> item, TCustomClass result) where TCustomClass : class
        {
            if (item.Value is not List<object> list || property.PropertyType == typeof(string))
            {
                property.SetValue(result, item.Value);
                return;
            }

            dynamic newList = Activator.CreateInstance(property.PropertyType);
            if (newList is null)
            {
                throw new FormatException($"can't create instance of {property.PropertyType}");
            }

            foreach (var someObject in list)
            {
                newList.Add(someObject is ExpandoObject exp
                    ? CreateAndFill<TCustomClass>(exp)
                    : someObject as TCustomClass);
            }

            property.SetValue(result, newList);
        }
        
        private static bool IsHaveProperAttribute(PropertyInfo property, string key)
        {
            return property.GetCustomAttributes()
                           .Select(x => x as BsonElementAttribute)
                           .Where(x => x != null)
                           .Any(x => x.ElementName.Equals(key, StringComparison.OrdinalIgnoreCase));
        }
    }
}
