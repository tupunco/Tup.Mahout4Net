using System;
using System.Text;
using System.Collections.Generic;

namespace Tup.Mahout4Net.Utils
{
    /// <summary>
    /// 
    /// </summary>
    public static class Arrays
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<T> asList<T>(params T[] obj)
        {
            return new List<T>(obj);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="separator"></param>
        /// <param name="array"></param>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string Join<T>(string separator, T[] array, int startIndex, int count)
        {
            if (array == null)
                return "null";
            if (array.Length == 0)
                return "[]";

            StringBuilder sb = new StringBuilder(array.Length * 7); // android-changed
            sb.Append('[');
            int maxIndex = count + startIndex;
            for (int i = 1; i < array.Length && i < maxIndex; i++)
            {
                if (i < startIndex)
                    continue;

                if (sb.Length > 1)
                    sb.Append(", ");

                sb.Append(array[i]);
            }
            sb.Append(']');
            return sb.ToString();
        }
        /// <summary>
        /// Compares the two arrays.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array1">the first {@code Object} array.</param>
        /// <param name="array2">the second {@code Object} array.</param>
        /// <returns>
        ///         {@code true} if both arrays are {@code null} or if the arrays have the
        ///         same length and the elements at each index in the two arrays are
        ///         equal according to {@code equals()}, {@code false} otherwise.
        /// </returns>
        public static bool equals<T>(T[] array1, T[] array2)
        {
            if (array1 == array2)
                return true;
            if (array1 == null || array2 == null || array1.Length != array2.Length)
                return false;

            var t = typeof(T);
            if (t.IsValueType)
            {
                for (int i = 0; i < array1.Length; i++)
                {
                    if (!array1[i].Equals(array2[i]))
                    {
                        return false;
                    }
                }
            }
            else
            {
                for (int i = 0; i < array1.Length; i++)
                {
                    T e1 = array1[i], e2 = array2[i];
                    if (!(e1 == null ? e2 == null : e1.Equals(e2)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="defaultValue"></param>
        public static void fill<T>(T[] array, T defaultValue)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = defaultValue;
            }
        }
        /// <summary>
        /// 用指定的 defaultValue 填充 array 数组中指定 范围的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="fromIx"></param>
        /// <param name="toIx"></param>
        /// <param name="defaultValue"></param>
        public static void fill<T>(T[] array, int fromIx, int toIx, T defaultValue)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (fromIx > toIx)
                throw new ArgumentOutOfRangeException("fromIx,toIx");
            if (fromIx < 0)
                throw new ArgumentOutOfRangeException("fromIx");
            if (toIx > array.Length)
                throw new ArgumentOutOfRangeException("toIx");

            for (int i = fromIx; i < toIx; i++)
            {
                array[i] = defaultValue;
            }
        }
    }
}
