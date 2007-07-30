using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Banshee.Cdrom.Windows.Interop
{
	/// <summary>
	/// A wrapper for a COM IPropertyStorage implementation.  Abstract
	/// for the purposes of this library since concrete implementations
	/// are provided for particular property sets.
	/// </summary>
	public abstract class PropertyStorage : ReadOnlyCollectionBase, IDisposable
	{
		private IPropertyStorage storage;
		private bool disposed = false;

		/// <summary>
		/// Internal constructor: instances can only be created by internal
		/// classes within the library.
		/// </summary>
		/// <param name="storage">IPropertyStorage implementation to wrap</param>
		internal PropertyStorage(IPropertyStorage storage)
		{
			this.storage = storage;
			ExtractProperties();
		}

		/// <summary>
		/// Destructor
		/// </summary>
		~PropertyStorage()
		{
			Dispose(false);
		}

		/// <summary>
		/// Disposes resources associated with this class.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Disposes resources associated with this class.
		/// </summary>
		/// <param name="disposing"><c>true</c> if disposing from the <c>Dispose</c>
		/// method, otherwise <c>false</c>.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					Marshal.ReleaseComObject(storage);
					storage = null;
				}
				disposed = true;
			}
		}

		/// <summary>
		/// Read the properties from the nasty IPropertyStorage instance.
		/// </summary>
		private void ExtractProperties()
		{
			IEnumSTATPROPSTG enumProps = null;
			storage.Enum(ref enumProps);

			int hRes = 0;
			STATPROPSTG propStg = new STATPROPSTG();
			int fetched = 0;
			object propValue = null;

			while (true)
			{
				hRes = enumProps.Next(1, ref propStg, out fetched);
				if ((!ComUtility.Failed(hRes)) && (fetched > 0))
				{
					// Get property value:
					PROPSPEC propSpecifier = new PROPSPEC();
					propSpecifier.ID_or_LPWSTR = (IntPtr) propStg.propid;
					propSpecifier.ulKind = PRPSPEC.PRSPEC_PROPID;

					storage.ReadMultiple(1, ref propSpecifier, out propValue);
					string name;
					if (propStg.lpwstrName != IntPtr.Zero)
					{
						name = Marshal.PtrToStringUni(propStg.lpwstrName);
					}
					else
					{
						name = String.Format("#{0}", propStg.propid);
					}
					
					Property property = new Property(propStg.propid, name, propValue, this);
					InnerList.Add(property);

					// have to do this, not a good design decision
					if (propStg.lpwstrName != IntPtr.Zero)
					{
						Marshal.FreeCoTaskMem(propStg.lpwstrName);
					}
				}								
				else
				{
					break;
				}
			}

		}

		/// <summary>
		/// Get the property at the specified 0-based index.
		/// </summary>
		public Property this[int index]
		{
			get
			{
				Property prop = (Property) InnerList[index];
				return prop;
			}
		}

		/// <summary>
		/// Updates the internal collection to reflect any changes
		/// made to the properties.
		/// </summary>
		internal void Update(Property prop)
		{
			object newValue = prop.Value;
			// dontchya love it
			PROPSPEC propSpecifier = new PROPSPEC();
			propSpecifier.ID_or_LPWSTR = (IntPtr) prop.Id;
			propSpecifier.ulKind = PRPSPEC.PRSPEC_PROPID;
			storage.WriteMultiple(1, ref propSpecifier, ref newValue, 0);
		}

		/// <summary>
		/// Gets the wrapped internal property storage object 
		/// </summary>
		/// <returns>Wrapped property storage object</returns>
		internal IPropertyStorage GetIPropertyStorage()
		{
			return storage;
		}

	}
}
