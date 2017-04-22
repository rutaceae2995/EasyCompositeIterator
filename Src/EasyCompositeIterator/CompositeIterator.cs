using System;
using System.Collections.Generic;

namespace EasyCompositeIterator
{
    /// <summary>
    /// Provides a iterator for instance of the class which is designed by the composite pattern.
    /// - The element class must have a get method of the element name.
    /// - The element class must have a get method of the value.
    /// - The element class must have a get method of the child elements.
    /// </summary>
    /// <typeparam name="T">Type of the element class.</typeparam>
    public sealed class CompositeIterator<T>
    {
        /// <summary>
        /// The top level instance of the composite.
        /// </summary>
        private readonly CompositeTemplate<T> core;

        /// <summary>
        /// Private constructor.
        /// </summary>
        /// <param name="template">Composite template.</param>
        private CompositeIterator(CompositeTemplate<T> template)
        {
            this.core = template;
        }

        /// <summary>
        /// Creates a CompositeIterator for the top level instance of the composite.
        /// </summary>
        /// <param name="target">Top level element.</param>
        /// <param name="getNameFunc">A method that gets the name of an element.</param>
        /// <param name="getValueFunc">A method that gets the value of an element.</param>
        /// <param name="getChildrenFunc">A method that gets the children of an element.</param>
        /// <returns>The created iterator instance.</returns>
        public static CompositeIterator<T> Create(T target, Func<T, string> getNameFunc, Func<T, object> getValueFunc, Func<T, IList<T>> getChildrenFunc)
        {
            return Create(new List<T> { target }, getNameFunc, getValueFunc, getChildrenFunc);
        }

        /// <summary>
        /// Creates a CompositeIterator for the top level instance of the composite.
        /// </summary>
        /// <param name="target">Target elements.</param>
        /// <param name="getNameFunc">A method that gets the name of an element.</param>
        /// <param name="getValueFunc">A method that gets the value of an element.</param>
        /// <param name="getChildrenFunc">A method that gets the children of an element.</param>
        /// <returns>The created iterator instance.</returns>
        public static CompositeIterator<T> Create(IList<T> target, Func<T, string> getNameFunc, Func<T, object> getValueFunc, Func<T, IList<T>> getChildrenFunc)
        {
            return new CompositeIterator<T>(new CompositeTemplate<T>(target, getNameFunc, getValueFunc, getChildrenFunc));
        }

        /// <summary>
        /// Returns a dynamic object of the element.
        /// </summary>
        /// <remarks>
        /// You can write "field.XXX" to get the value of the element name "XXX".
        /// </remarks>
        /// <returns>The dynamic object.</returns>
        public dynamic AsDynamic()
        {
            return this.core;
        }

        /// <summary>
        /// Extracts the element.
        /// </summary>
        /// <param name="elementName">Target element name.</param>
        /// <returns>Extracted element.</returns>
        public T Extract(string elementName)
        {
            return this.core.Extract(elementName);
        }

        /// <summary>
        /// Extracts the value specified by an element name.
        /// If the tag does not exist or exist more than two, throws exception.
        /// </summary>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="elementName">An element name.</param>
        /// <returns>Value.</returns>
        public TValue GetValue<TValue>(string elementName)
        {
            return (TValue)this.core.GetSingleRecordValue(elementName);
        }

        /// <summary>
        /// Extracts the values specified by an element name.
        /// </summary>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="elementName">An element name.</param>
        /// <returns>Values.</returns>
        public TValue[] GetValues<TValue>(string elementName)
        {
            return this.core.GetMultiValues<TValue>(elementName);
        }

        /// <summary>
        /// Gets a value specified by an element name.
        /// If the element name does not exist, returns false.
        /// </summary>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="elementName">An element name.</param>
        /// <param name="result">Value.</param>
        /// <returns>True of the element name exists; otherwise, false.</returns>
        public bool TryGetValue<TValue>(string elementName, out TValue result)
        {
            return this.core.TryGetValue(elementName, out result);
        }

        /// <summary>
        /// Iterates the child elements of the specified element and creates a instance.
        /// The target element must be single and have child elements.
        /// </summary>
        /// <typeparam name="TValue">Type of the return value.</typeparam>
        /// <param name="elementName">An element name.</param>
        /// <param name="func">Iterator function.</param>
        /// <returns>The created instance.</returns>
        public TValue SingleChild<TValue>(string elementName, Func<CompositeIterator<T>, TValue> func)
        {
            var iterator = this.core.CreateSubIteratorSingle(elementName);
            if (iterator == null)
            {
                throw new InvalidOperationException("Target element name does not exists.");
            }

            return func(iterator);
        }

        /// <summary>
        /// Iterates the child elements of the specified element and creates instances.
        /// The target element may exist more than two and must have child elements.
        /// </summary>
        /// <typeparam name="TValue">Type of the return value.</typeparam>
        /// <param name="elementName">An element name.</param>
        /// <param name="func">Iterator function.</param>
        /// <returns>Returns an IEnumerator for this enumerable instance.</returns>
        public IEnumerable<TValue> MultiChild<TValue>(string elementName, Func<CompositeIterator<T>, TValue> func)
        {
            foreach (var target in this.core.CreateSubIteratorMultiple(elementName))
            {
                yield return func(target);
            }
        }

        /// <summary>
        /// Splits the child element and iterates this subset elements.
        /// e.g.) Coord / X,Y,X,Y,X,Y => Split("Coord", 2, func)
        /// </summary>
        /// <param name="count">The element count of the subset.</param>
        /// <returns>Returns an IEnumerator for the subset.</returns>
        public IEnumerable<CompositeIterator<T>> Split(int count)
        {
            return this.core.Split(count);
        }
    }
}