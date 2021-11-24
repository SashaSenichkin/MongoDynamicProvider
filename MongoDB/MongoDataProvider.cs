using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
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
    public class MongoDataProvider<TEntity> : BaseNoSqlProvider<TEntity> where TEntity : class, new()
    {
        private readonly IMongoCollection<BsonDocument> _bsonCollection;
        public MongoDataProvider(IMongoDbSettings settings, string nameCollection) : base(settings, nameCollection)
        {
            var client = new MongoClient(settings.ConnectionString);
            var mongoBase = client.GetDatabase(settings.DatabaseName);
            _bsonCollection = mongoBase.GetCollection<BsonDocument>(nameCollection);
        }
        
        /// <summary>
        /// base method to work. supports mongo filers 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>list of entities we, can parse correctly</returns>
        public async Task<List<TEntity>> GetCollectionAsync(FilterDefinition<TEntity> filter = null)
        {
            var filterTypes = filter?.Render(BsonSerializer.SerializerRegistry.GetSerializer<TEntity>(), BsonSerializer.SerializerRegistry) ?? Builders<BsonDocument>.Filter.Empty;

            var foundElems = await _bsonCollection.FindAsync(filterTypes);
            var foundElemsList = await foundElems.ToListAsync();
            var result = foundElemsList.Select(Convert).Where(x => x != null).ToList();
            return result;
        }

        /// <summary>
        /// experimental feature.. can support linq queries, as documentation says)
        /// </summary>
        /// <param name="qFilter"></param>
        /// <returns>list of entities we, can parse correctly</returns>
        public async Task<List<TEntity>> GetCollectionAsync(Func<IQueryable, IEnumerable<BsonDocument>> qFilter)
        {
            var filter = Builders<TEntity>.Filter.Empty;
            var foundElems = qFilter(_bsonCollection.AsQueryable()).Where(x => filter.Inject());
            var foundElemsList = foundElems.ToList();
            var result = foundElemsList.Select(Convert).Where(x => x != null).ToList();
            return result;
        }

        /// <summary>
        /// reflection parser start. swallow exceptions. use DebugConvert if you want to see errors with entity
        /// </summary>
        /// <param name="source">document to parse</param>
        /// <returns>entity of your type or null</returns>
        public static TEntity Convert(BsonDocument source)
        {
            try
            {
                var expandFromBson = BsonSerializer.Deserialize<ExpandoObject>(source);
                var result = CreateAndFill<TEntity>(expandFromBson);
                return result;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        
        /// <summary>
        /// reflection to Debug problem entities 
        /// </summary>
        /// <param name="source">document to parse</param>
        /// <returns>entity of your type or Exception</returns>
        /// <exception cref="FormatException">some unsupported cases from mongo bson</exception>
        public static TEntity DebugConvert(BsonDocument source)
        {
            var expandFromBson = BsonSerializer.Deserialize<ExpandoObject>(source);
            var result = CreateAndFill<TEntity>(expandFromBson);
            return result;
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
            if (item.Value is List<object> list && property.PropertyType != typeof(string))
            {
                dynamic newList = Activator.CreateInstance(property.PropertyType);
                if (newList is null)
                {
                    throw new FormatException($"can't create instance of {property.PropertyType}" );
                }
                
                foreach (var someObject in list)
                {
                    newList.Add(someObject is ExpandoObject exp
                        ? CreateAndFill<TCustomClass>(exp)
                        : someObject as TCustomClass);
                }

                property.SetValue(result, newList);
            }
            else
            {
                property.SetValue(result, item.Value);
            }
        }
        
        private static bool IsHaveProperAttribute(PropertyInfo property, string key)
        {
            return property.GetCustomAttributes().Select(x => x as BsonElementAttribute).Where(x => x != null)
                                                 .Any(x => x.ElementName.Equals(key, StringComparison.OrdinalIgnoreCase));
        }
    }
}
