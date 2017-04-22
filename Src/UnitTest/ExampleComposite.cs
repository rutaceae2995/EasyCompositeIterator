using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace UnitTest
{
    /// <summary>
    /// Component class of the composite pattern.
    /// This is a class for the unit test.
    /// </summary>
    [DebuggerDisplay("{ElementName}")]
    public abstract class Component
    {
        /// <summary>
        /// Gets the element name.
        /// </summary>
        public string ElementName
        {
            get;
            protected set;
        }

        /// <summary>
        /// Tests if this element has children.
        /// </summary>
        public abstract bool HasChild
        {
            get;
        }

        /// <summary>
        /// Gets the child elements.
        /// </summary>
        public abstract IList<Component> Children
        {
            get;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public abstract object Value
        {
            get;
        }

        /// <summary>
        /// Do anything operation for test.
        /// </summary>
        /// <returns>Return value.</returns>
        public abstract int Operate();
    }

    /// <summary>
    /// Leaf class of the composite pattern.
    /// </summary>
    [DebuggerDisplay("{ElementName} = {Value}")]
    public sealed class Leaf : Component
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">A name.</param>
        /// <param name="value">A value.</param>
        public Leaf(string name, object value)
        {
            this.ElementName = name;
            this.Value = value;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public override object Value
        {
            get;
        }

        /// <summary>
        /// Tests if this element has children (always false).
        /// </summary>
        public override bool HasChild => false;

        /// <summary>
        /// Leaf must not have children (always null).
        /// </summary>
        public override IList<Component> Children => null;

        /// <summary>
        /// Gets the length of the element name.
        /// (Example)
        /// </summary>
        /// <returns>Return value.</returns>
        public override int Operate()
        {
            return this.ElementName.Length;
        }
    }

    /// <summary>
    /// Composite class of the composite pattern.
    /// </summary>
    [DebuggerDisplay("{ElementName} (Composite)")]
    public sealed class Composite : Component
    {
        /// <summary>
        /// Child elements.
        /// </summary>
        private readonly IList<Component> children = new List<Component>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="elementName">An element name.</param>
        public Composite(string elementName)
        {
            this.ElementName = elementName;
        }

        /// <summary>
        /// Tests if the element can have children (always true).
        /// </summary>
        public override bool HasChild => true;

        /// <summary>
        /// Gets the child elements.
        /// </summary>
        public override IList<Component> Children => this.children;

        /// <summary>
        /// Gets the value (always null).
        /// </summary>
        public override object Value => null;

        /// <summary>
        /// Add a child.
        /// </summary>
        /// <param name="child">A component.</param>
        public void AddChild(Component child)
        {
            this.children.Add(child);
        }

        /// <summary>
        /// Gets the total length of the all child element name.
        /// (Example)
        /// </summary>
        /// <returns>Return value.</returns>
        public override int Operate()
        {
            return this.Children.Select(e => e.Operate()).Sum();
        }
    }
}