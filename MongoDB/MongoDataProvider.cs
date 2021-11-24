﻿using MongoDB.Bson;
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
    public class MongoDataProvider<TEntity> : BaseNoSqlProvider<BsonDocument> where TEntity : class, new()
    {
        public MongoDataProvider(IMongoDbSettings settings, string nameCollection) : base(settings, nameCollection)
        {
        }
        
        /// <summary>
        /// base method to work. supports mongo filers 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="errors">all errors, raised while, null if you don't care</param>
        /// <returns>list of entities we, can parse correctly</returns>
        public async Task<List<TEntity>> GetCollectionAsync(FilterDefinition<TEntity> filter = null, List<Exception> errors = null)
        {
            var filterTypes = filter?.Render(BsonSerializer.SerializerRegistry.GetSerializer<TEntity>(), BsonSerializer.SerializerRegistry) ?? Builders<BsonDocument>.Filter.Empty;

            var foundElems = await GetContext().Source.FindAsync(filterTypes).ConfigureAwait(false);
            var foundElemsList = await foundElems.ToListAsync().ConfigureAwait(false);
            var result = foundElemsList.Select(x => Convert(x, errors)).Where(x => x != null).ToList();
            return result;
        }

        /// <summary>
        /// experimental feature.. can support linq queries, as documentation says)
        /// </summary>
        /// <param name="qFilter"></param>
        /// <returns>list of entities we, can parse correctly</returns>
        public async Task<List<TEntity>> GetCollectionAsync(Func<IQueryable, IEnumerable<BsonDocument>> qFilter, List<Exception> errors = null)
        {
            var filter = Builders<TEntity>.Filter.Empty;
            var foundElems = qFilter(GetContext().Source.AsQueryable()).Where(x => filter.Inject());
            var foundElemsList = foundElems.ToList();
            var result = foundElemsList.Select(x => Convert(x, errors)).Where(x => x != null).ToList();
            return result;
        }

        /// <summary>
        /// reflection parser start. swallow exceptions. use DebugConvert if you want to see errors with entity
        /// </summary>
        /// <param name="source">document to parse</param>
        /// <param name="errors">all errors, raised while, null if you don't care</param>
        /// <returns>entity of your type or null</returns>
        /// <exception cref="FormatException">some unsupported cases from mongo bson</exception>
        public static TEntity Convert(BsonDocument source, List<Exception> errors)
        {
            try
            {
                var expandFromBson = BsonSerializer.Deserialize<ExpandoObject>(source);
                var result = CreateAndFill<TEntity>(expandFromBson);
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
