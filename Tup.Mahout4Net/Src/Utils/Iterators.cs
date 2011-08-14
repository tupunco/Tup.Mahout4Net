using System;
using System.Collections.Generic;

namespace Tup.Mahout4Net.Utils
{
    /// <summary>
    /// 
    /// </summary>
    public static class Iterators
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="F"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="fromIterator"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public static IEnumerable<TResult> Transform<F, TResult>(this IEnumerable<F> fromIterator,
                                      Func<F, TResult> function)
        {
            if (fromIterator == null)
                throw new ArgumentNullException("fromIterator");
            if (function == null)
                throw new ArgumentNullException("function");

            foreach (var item in fromIterator)
            {
                yield return function(item);
            }
        }
    }
}
