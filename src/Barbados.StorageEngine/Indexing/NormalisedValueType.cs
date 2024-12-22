namespace Barbados.StorageEngine.Indexing
{
	internal enum NormalisedValueType : byte
	{
		/* Keys of different types in the BTree are sorted according to the values in here
		*/

		Min = 1,
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
