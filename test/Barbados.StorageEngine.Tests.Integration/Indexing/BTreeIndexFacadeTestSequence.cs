using Barbados.StorageEngine.Tests.Integration.Collections;

using Xunit.Abstractions;

namespace Barbados.StorageEngine.Tests.Integration.Indexing
{
	public sealed class BTreeIndexFacadeTestSequence : IXunitSerializable
	{
		public string IndexField { get; private set; }
		public int KeyMaxLength { get; private set; }
		public bool UseDefaultKeyMaxLength { get; private set; }
		public BarbadosCollectionFacadeTestSequence DocumentSequence { get; private set; }

		public BTreeIndexFacadeTestSequence() : this(default!, default!, default!)
		{

		}

		internal BTreeIndexFacadeTestSequence(string indexedField, BarbadosCollectionFacadeTestSequence seq)
		{
			IndexField = indexedField;	
			KeyMaxLength = -1;
			UseDefaultKeyMaxLength = true;
			DocumentSequence = seq;
		}

		internal BTreeIndexFacadeTestSequence(string indexedField, int keyMaxLength, BarbadosCollectionFacadeTestSequence seq)
		{
			IndexField = indexedField;
			KeyMaxLength = keyMaxLength;
			UseDefaultKeyMaxLength = false;
			DocumentSequence = seq;
		}

		public void Serialize(IXunitSerializationInfo info)
		{
			info.AddValue(nameof(IndexField), IndexField);
			info.AddValue(nameof(KeyMaxLength), KeyMaxLength);
			info.AddValue(nameof(UseDefaultKeyMaxLength), UseDefaultKeyMaxLength);
			info.AddValue(nameof(DocumentSequence), DocumentSequence);
		}

		public void Deserialize(IXunitSerializationInfo info)
		{
			IndexField = info.GetValue<string>(nameof(IndexField));
			KeyMaxLength = info.GetValue<int>(nameof(KeyMaxLength));
			UseDefaultKeyMaxLength = info.GetValue<bool>(nameof(UseDefaultKeyMaxLength));
			DocumentSequence = info.GetValue<BarbadosCollectionFacadeTestSequence>(nameof(DocumentSequence));
		}

		public override string ToString() => DocumentSequence.Name;
	}
}
