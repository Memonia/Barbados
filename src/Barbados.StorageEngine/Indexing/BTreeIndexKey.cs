using System.Diagnostics;

using Barbados.StorageEngine.Documents.Binary;

namespace Barbados.StorageEngine.Indexing
{
	internal readonly ref struct BTreeIndexKey
	{
		public bool IsTrimmed { get; }

		public NormalisedValueSpan Separator { get; }

		public BTreeIndexKey(NormalisedValueSpan separator, bool isTrimmed)
		{
			Debug.Assert(Separator.Bytes.Length <= Constants.IndexKeyMaxLength);
			Separator = separator;
			IsTrimmed = isTrimmed;
		}
	}
}
