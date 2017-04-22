using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace EasyCompositeIterator
{
    /// <summary>
    /// Implements the template for the CompositeIterator class.
    /// This class is used only for internal.
    /// </summary>
    /// <typeparam name="T">The type of the class designed by the composite pattern.</typeparam>
    internal sealed class CompositeTemplate<T> : DynamicObject
    {
        /// <summary>
        /// The list of the top level elements.
        /// </summary>
        private readonly IList<T> coreElements;

        /// <summary>
        /// A method that gets the name of an element.
        /// </summary>
        private readonly Func<T, string> getNameFunc;

        /// <summary>
        /// A method that gets the value of an element.
        /// </summary>
        private readonly Func<T, object> getValueFunc;

        /// <summary>
        /// A method that gets the child elements of an element.
        /// </summary>
        private readonly Func<T, IList<T>> getChildrenFunc;

        /// <summary>
        /// Provides the implementation of getting a member.
        /// </summary>
        /// <param name="binder">The binder provided.</param>
        /// <param name="result">The result of the get operation.</param>
        /// <returns>true if the operation is complete; otherwise, false.</returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var target = this.coreElements.FirstOrDefault(e => this.getNameFunc(e) == binder.Name);
            if (target == null)
            {
                result = null;
                return false;
            }

            var children = this.getChildrenFunc(target);
            if (children != null && children.Count != 0)
            {
                result = null;
            }
            else
            {
                result = this.getValueFunc(target);
            }

            return true;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="compositeRecords">Target element.</param>
        /// <param name="getNameFunc">A method that gets the name of an element.</param>
        /// <param name="getValueFunc">A method that gets the value of an element.</param>
        /// <param name="getChildrenFunc">A method that gets the child elements of an element.</param>
        internal CompositeTemplate(IList<T> compositeRecords, Func<T, string> getNameFunc, Func<T, object> getValueFunc, Func<T, IList<T>> getChildrenFunc)
        {
            if (compositeRecords == null)
            {
                throw new ArgumentNullException(nameof(compositeRecords));
            }

            if (getChildrenFunc == null)
            {
                throw new ArgumentNullException(nameof(getChildrenFunc));
            }

            if (getValueFunc == null)
            {
                throw new ArgumentNullException(nameof(getValueFunc));
            }

            if (getNameFunc == null)
            {
                throw new ArgumentNullException(nameof(getNameFunc));
            }

            this.getChildrenFunc = getChildrenFunc;
            this.getValueFunc = getValueFunc;
            this.getNameFunc = getNameFunc;
            this.coreElements = compositeRecords;
        }

        /// <summary>
        /// Extracts the element.
        /// </summary>
        /// <param name="elementName">Target element name.</param>
        /// <returns>Extracted element.</returns>
        internal T Extract(string elementName)
        {
            return this.coreElements.FirstOrDefault(e => this.getNameFunc(e) == elementName);
        }

        /// <summary>
        /// Extracts the value specified by an element name.
        /// If the tag does not exist or exist more than two, throw exception.
        /// </summary>
        /// <param name="elementName">An element name.</param>
        /// <returns>Value.</returns>
        internal object GetSingleRecordValue(string elementName)
        {
            var record = this.coreElements.Single(e => this.getNameFunc(e) == elementName);
            if (record == null)
            {
                return null;
            }

            return this.getValueFunc(record);
        }

        /// <summary>
        /// Gets a value specified by an element name.
        /// If the element name does not exist, returns false.
        /// </summary>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="elementName">An element name.</param>
        /// <param name="result">Value.</param>
        /// <returns>True of the element name exists; otherwise, false.</returns>
        internal bool TryGetValue<TValue>(string elementName, out TValue result)
        {
            var target = this.coreElements.FirstOrDefault(e => this.getNameFunc(e) == elementName);
            if (target == null)
            {
                result = default(TValue);
                return false;
            }

            result = (TValue)this.getValueFunc(target);
            return true;
        }

        /// <summary>
        /// Extracts the values of the specified child element.
        /// </summary>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="elementName">An element name.</param>
        /// <returns>Array of the value.</returns>
        internal TValue[] GetMultiValues<TValue>(string elementName)
        {
            return this.coreElements
                .Where(e => this.getNameFunc(e) == elementName)
                .Select(record => (TValue)this.getValueFunc(record))
                .ToArray();
        }

        /// <summary>
        /// Provides a new iterator for a sub element specified by the elementName.
        /// If the element name exists more than two, throws exception.
        /// </summary>
        /// <param name="elementName">An element name.</param>
        /// <returns>Iterator.</returns>
        internal CompositeIterator<T> CreateSubIteratorSingle(string elementName)
        {
            var target = this.coreElements.Single(e => this.getNameFunc(e) == elementName);
            if (target == null)
            {
                return null;
            }

            return this.CreateSubIterator(target);
        }

        /// <summary>
        /// Provides the enumeration of the iterators for a sub element specified by the elementName.
        /// </summary>
        /// <param name="elementName">An element name.</param>
        /// <returns>The enumeration of the iterators.</returns>
        internal IEnumerable<CompositeIterator<T>> CreateSubIteratorMultiple(string elementName)
        {
            foreach (var record in this.coreElements.Where(e => this.getNameFunc(e) == elementName))
            {
                yield return this.CreateSubIterator(record);
            }
        }

        /// <summary>
        /// Splits the child element and iterates this subset elements.
        /// </summary>
        /// <param name="count">The element count of the subset.</param>
        /// <returns>Returns an IEnumerator for the subset.</returns>
        internal IEnumerable<CompositeIterator<T>> Split(int count)
        {
            if (count <= 0)
            {
                yield break;
            }

            for (var i = 0; i < this.coreElements.Count; i++)
            {
                var internalList = new List<T>(count);
                for (var j = 0; j < count; j++)
                {
                    if (this.coreElements.Count <= i)
                    {
                        break;
                    }

                    internalList.Add(this.coreElements[i]);
                    i++;
                }

                yield return CompositeIterator<T>.Create(internalList, this.getNameFunc, this.getValueFunc, this.getChildrenFunc);
            }
        }

        /// <summary>
        /// Provides a new iterator for the element.
        /// </summary>
        /// <param name="targetRoot">The top level element.</param>
        /// <returns>New iterator.</returns>
        private CompositeIterator<T> CreateSubIterator(T targetRoot)
        {
            if (targetRoot == null)
            {
                throw new InvalidOperationException(nameof(targetRoot));
            }

            var children = this.getChildrenFunc(targetRoot);
            if (children == null)
            {
                throw new InvalidOperationException("Target element is not composite.");
            }

            return CompositeIterator<T>.Create(children, this.getNameFunc, this.getValueFunc, this.getChildrenFunc);
        }
    }
}