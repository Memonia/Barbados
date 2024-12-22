using Barbados.StorageEngine.Tests.Integration.Collections;

namespace Barbados.StorageEngine.Tests.Integration.Indexing
{
	public sealed class BTreeIndexFacadeTestSequence
	{
		public string IndexField { get; private set; }
		public int KeyMaxLength { get; private set; }
		public bool UseDefaultKeyMaxLength { get; private set; }
		public BarbadosCollectionFacadeTestSequence DocumentSequence { get; private set; }

		public BTreeIndexFacadeTestSequence(string indexedField, BarbadosCollectionFacadeTestSequence seq)
		{
			IndexField = indexedField;	
			KeyMaxLength = -1;
			UseDefaultKeyMaxLength = true;
			DocumentSequence = seq;
		}

		public BTreeIndexFacadeTestSequence(string indexedField, int keyMaxLength, BarbadosCollectionFacadeTestSequence seq)
		{
			IndexField = indexedField;
			KeyMaxLength = keyMaxLength;
			UseDefaultKeyMaxLength = false;
			DocumentSequence = seq;
		}

		public override string ToString() => DocumentSequence.Name;
	}
}
