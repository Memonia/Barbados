using System;

using Barbados.Documents;
using Barbados.StorageEngine.Storage.Paging;

namespace Barbados.StorageEngine.Indexing
{
	internal sealed class BTreeIndexInfo
	{
		public required ObjectId CollectionId { get; init; }
		public required PageHandle RootHandle { get; init; }
		public required BarbadosKey IndexField { get; init; }

		public required int KeyMaxLength
		{
			get => _keyMaxLength;
			init
			{
				if (value <= 0 || value > Constants.IndexKeyMaxLength)
				{
					throw new ArgumentException(
						$"Index key maximum length must be between 1 and {Constants.IndexKeyMaxLength} bytes"
					);
				}

				_keyMaxLength = value;
			}
		}

		private readonly int _keyMaxLength;
	}
}
