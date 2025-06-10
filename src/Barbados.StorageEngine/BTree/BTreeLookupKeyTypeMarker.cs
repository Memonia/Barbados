namespace Barbados.StorageEngine.BTree
{
	internal enum BTreeLookupKeyTypeMarker : byte
	{
		/* Keys of different types in the BTree are sorted in order of entries in here
		*/

		// Separates a part of the tree dedicated for chunk storage
		KeyChunk = 1,
		DataChunk,

		// Separates a part of the tree dedicated for storage of internal data of an upper level
		External = 4,

		Min = 16,
		Int8,
		Int16,
		Int32,
		Int64,
		UInt8,
		UInt16,
		UInt32,
		UInt64,
		Float32,
		Float64,
		DateTime,
		Boolean,
		String,
		Max = 255
	}
}
