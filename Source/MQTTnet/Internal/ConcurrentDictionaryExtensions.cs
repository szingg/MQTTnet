using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MQTTnet.Internal
{
    public static class ConcurrentDictionaryExtensions
    {
        /// <summary>
        /// Provides a non-thread-safe approach to access ConcurrentDictionary properties.
        /// This method should only be called in cases where performance is more important than consistency.
        /// 
        /// Calls to the Properties Count, IsEmpty, Keys and Values are extremely expensive for they acquire all (default 31) dictionary locks.
        /// Methods like TryAdd are less expensive since they acquire only 1 lock.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="propertySelector"></param>
        /// <returns></returns>
        public static TProperty GetNonBlocking<TKey, TValue, TProperty>(
            this ConcurrentDictionary<TKey, TValue> dictionary, 
            Expression<Func<ConcurrentDictionary<TKey, TValue>, TProperty>> propertySelector
        )
        {
            MemberExpression member = propertySelector?.Body as MemberExpression ?? 
                throw new ArgumentException(string.Format("Expression '{0}' must refer to a property.", propertySelector.ToString()));

            var propertyName = member.Member.Name;
            switch(propertyName)
            {
                case "Count":
                    return (TProperty)(object)dictionary.Skip(0).Count();
                case "IsEmpty":
                    return (TProperty)(object)(dictionary.Skip(0).Any() == false);
                case "Keys":
                    return (TProperty)(object)dictionary.Select(x => x.Key).ToList();
                case "Values":
                    return (TProperty)(object)dictionary.Select(x => x.Value).ToList();
                default:
                    return propertySelector.Compile().Invoke(dictionary);
            }

        }
    }
}
