using Barbados.StorageEngine.Documents.Serialisation.Metadata;

namespace Barbados.StorageEngine.Documents.Serialisation
{
	internal partial class RadixTreeBuffer
	{
		private readonly struct NodeInfo
		{
			public int Offset { get; }
			public PrefixDescriptor Descriptor { get; }

			public NodeInfo(int offset, PrefixDescriptor descriptor)
			{
				Offset = offset;
				Descriptor = descriptor;
			}
		}
	}
}
