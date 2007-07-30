using System;

namespace Banshee.Cdrom.Windows.Interop
{
	/// <summary>
	/// Collection of properties associated with a disc recorder.
	/// </summary>
	public class DiscRecorderProperties : PropertyStorage
	{
		internal DiscRecorderProperties(IPropertyStorage propertyStorage) : base(propertyStorage)
		{
		}
	}
}
