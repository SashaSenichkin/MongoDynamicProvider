using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Stp.Tools.MongoDB.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Stp.Tools.MongoDB
{
    public class MongoDataProvider<T> : BaseNoSqlProvider<T> where T : class, new()
    {
        private IMongoCollection<BsonDocument> bsonCollection;
        public MongoDataProvider(IMongoDbSettings settings, string nameCollection) : base(settings, nameCollection)
        {
            var client = new MongoClient(settings.ConnectionString);
            var mongoBase = client.GetDatabase(settings.DatabaseName);
            bsonCollection = mongoBase.GetCollection<BsonDocument>(nameCollection);
        }

        public async Task<IEnumerable<T>> GetCollectionAsync(FilterDefinition<T> filter = null)
        {
            IEnumerable<T> result = new List<T>();
            var filterTypes = filter?.Render(BsonSerializer.SerializerRegistry.GetSerializer<T>(), BsonSerializer.SerializerRegistry) ?? Builders<BsonDocument>.Filter.Empty;

            var foundElems = await bsonCollection.FindAsync(filterTypes);
            var foundElemsList = await foundElems.ToListAsync();
            result = foundElemsList.Select(x => Convert(x));
            return result;
        }
        public async Task<IEnumerable<T>> GetCollectionAsync(Func<IQueryable, IEnumerable<BsonDocument>> qFilter)
        {
            IEnumerable<T> result = new List<T>();
            var filter = Builders<T>.Filter.Empty;
            var foundElems = qFilter(bsonCollection.AsQueryable()).Where(x => filter.Inject());
            var foundElemsList = foundElems.ToList();
            result = foundElemsList.Select(x => Convert(x));
            return result;
        }


        public static T Convert(BsonDocument source)
        {
            var expandFromBson = BsonSerializer.Deserialize<ExpandoObject>(source);
            var result = CreateAndFill<T>(expandFromBson);
            return result;
        }

        private static Y CreateAndFill<Y>(ExpandoObject source)
        {
            var result = Activator.CreateInstance<Y>();
            var props = typeof(Y).GetProperties();
            foreach (var item in source.Where(x => x.Value != null))
            {
                try
                {
                    var property = props.FirstOrDefault(x => IsHaveProperAttribute(x, item.Key));
                    if (property == null)
                        property = props.FirstOrDefault(x => x.Name.Equals(item.Key, StringComparison.OrdinalIgnoreCase)); 

                    if (property != null)
                        SetValue(property, item, result);
                }
                catch (Exception e)
                {
                    //continue;
                }
            }
            return result;
        }

      private static void SetValue<Y>(PropertyInfo property, KeyValuePair<string, object> item, Y result)
        {
            if (item.Value is List<object> list && property.PropertyType != typeof(string))
            {
                dynamic newList = Activator.CreateInstance(property.PropertyType);
                foreach (var someObject in list)
                    AddValueToList(newList, someObject);

                property.SetValue(result, newList);
            }
            else
            {
                property.SetValue(result, item.Value);
            }
        }
        public static void AddValueToList<Y>(List<Y> list, object obj) where Y : class
        {
            Y item;
            if (obj is ExpandoObject exp)
            {
                item = CreateAndFill<Y>(exp);
            }
            else
            {
                item = obj as Y;
            }

            list.Add(item);
        }
        private static bool IsHaveProperAttribute(PropertyInfo x, string key)
        {
            var attributes = x.GetCustomAttributes().Select(x => x as BsonElementAttribute).Where(x => x != null);
            return attributes.Any(x => x.ElementName.Equals(key, StringComparison.OrdinalIgnoreCase));
        }
    }
}
