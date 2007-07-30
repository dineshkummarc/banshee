using System;
using System.Text;

namespace Banshee.Cdrom.Windows.Interop
{
	/// <summary>
	/// A property from an IPropertyStorage collection.
	/// </summary>
	public class Property
	{
		private readonly int id;
		private readonly string name;
		private object propValue;
		PropertyStorage owner;

		/// <summary>
		/// Internal constructor: instances of this class are created
		/// by the owning IPropertyStorage instance.
		/// </summary>
		/// <param name="id">Id of the property</param>
		/// <param name="name">Name of the property</param>
		/// <param name="propValue">Value of the property</param>
		/// <param name="owner">Owning collection</param>
		internal Property(int id, string name, object propValue, PropertyStorage owner)
		{
			this.id = id;
			this.name = name;
			this.propValue = propValue;
			this.owner = owner;
		}

		/// <summary>
		/// Gets the ID of this property.
		/// </summary>
		public int Id
		{
			get
			{
				return id;
			}
		}

		/// <summary>
		/// Gets the name of this property.
		/// </summary>
		public string Name
		{
			get
			{
				return name;
			}
		}

		/// <summary>
		/// Gets/sets the value of this property.
		/// </summary>
		public object Value
		{
			get
			{
				return propValue;
			}
			set
			{
				propValue = value;
				owner.Update(this);
			}
		}

		/// <summary>
		/// Gets a string representation of this object.
		/// </summary>
		/// <returns>string representation</returns>
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder("Property: ");
			builder.Append(id);
			builder.Append(", ");
			builder.Append(name);
			builder.Append(", ");
			builder.Append(Value);
			return builder.ToString();
		}

	}
}
