using Barbados.StorageEngine.Tests.Integration.Collections;

using Xunit.Abstractions;

namespace Barbados.StorageEngine.Tests.Integration.Indexing
{
	public sealed class BTreeIndexTestSequence : IXunitSerializable
	{
		public string IndexedField { get; private set; }
		public int KeyMaxLength { get; private set; }
		public bool UseDefaultKeyMaxLength { get; private set; }
		public BarbadosCollectionTestSequence DocumentSequence { get; private set; }

		public BTreeIndexTestSequence() : this(default!, default!, default!)
		{

		}

		internal BTreeIndexTestSequence(string indexedField, BarbadosCollectionTestSequence seq)
		{
			IndexedField = indexedField;	
			KeyMaxLength = -1;
			UseDefaultKeyMaxLength = true;
			DocumentSequence = seq;
		}

		internal BTreeIndexTestSequence(string indexedField, int keyMaxLength, BarbadosCollectionTestSequence seq)
		{
			IndexedField = indexedField;
			KeyMaxLength = keyMaxLength;
			UseDefaultKeyMaxLength = false;
			DocumentSequence = seq;
		}

		public void Serialize(IXunitSerializationInfo info)
		{
			info.AddValue(nameof(IndexedField), IndexedField);
			info.AddValue(nameof(KeyMaxLength), KeyMaxLength);
			info.AddValue(nameof(UseDefaultKeyMaxLength), UseDefaultKeyMaxLength);
			info.AddValue(nameof(DocumentSequence), DocumentSequence);
		}

		public void Deserialize(IXunitSerializationInfo info)
		{
			IndexedField = info.GetValue<string>(nameof(IndexedField));
			KeyMaxLength = info.GetValue<int>(nameof(KeyMaxLength));
			UseDefaultKeyMaxLength = info.GetValue<bool>(nameof(UseDefaultKeyMaxLength));
			DocumentSequence = info.GetValue<BarbadosCollectionTestSequence>(nameof(DocumentSequence));
		}

		public override string ToString() => DocumentSequence.Name;
	}
}
