using System;
using System.Collections.Generic;
using System.Linq;

using Barbados.StorageEngine.Documents.Exceptions;
using Barbados.StorageEngine.Documents.Serialisation.Metadata;
using Barbados.StorageEngine.Documents.Serialisation.Values;

namespace Barbados.StorageEngine.Documents.Serialisation
{
	internal partial class RadixTreeBuffer
	{
		public partial class Builder
		{
			public static RadixTreeBuffer Build(RadixTreeNode root)
			{
				/* See 'RadixTreeBuffer' for the structure of the buffer */

				var relativeNodeOffsets = new Dictionary<int, int>();
				var prefixTableLength = 0;
				var valueTableLength = 0;
				try
				{
					checked
					{
						// Skip root
						foreach (var info in root.EnumerateNodesBreadthFirst().Skip(1))
						{
							if (info.IsFirstChild)
							{
								relativeNodeOffsets.Add(info.NodeId, prefixTableLength);
							}

							prefixTableLength += PrefixDescriptor.BinaryLength + info.Prefix.Length;
							if (info.Value is not null)
							{
								prefixTableLength += ValueDescriptor.BinaryLength;
								valueTableLength += info.Value.GetLength();
							}
						}
					}
				}

				catch (OverflowException oe)
				{
					throw new BarbadosDocumentSerialisationException(
						$"Resulting buffer exceeded the maximum length of {int.MaxValue}", oe
					);
				}

				if (prefixTableLength > MaxPrefixTableLength)
				{
					throw new BarbadosDocumentSerialisationException(
						$"The prefix table exceeded maximum length of {MaxPrefixTableLength} bytes" 
					);

				}

				if (valueTableLength > MaxValueTableLength)
				{
					throw new BarbadosDocumentSerialisationException(
						$"The value table exceeded maximum length of {MaxValueTableLength} bytes"
					);
				}

				var buffer = new byte[PrefixTableOffset + prefixTableLength + valueTableLength];
				var bspan = buffer.AsSpan();
				var currentPrefixTableOffset = PrefixTableOffset;
				var currentValueTableOffset = currentPrefixTableOffset + prefixTableLength;
				var currentRelativeValueTableOffset = 0;

				ValueBufferRawHelpers.WriteInt32(bspan, currentValueTableOffset);
				foreach (var info in root.EnumerateNodesBreadthFirst().Skip(1))
				{
					var prefixDescriptor = new PrefixDescriptor(
						info.Prefix.Length,
						info.HasChildren ? relativeNodeOffsets[info.FirstChildId] : 0,
						info.Value is not null,
						info.IsLastChild
					);

					ValueBufferRawHelpers.WriteUInt32(bspan[currentPrefixTableOffset..], prefixDescriptor.Bits);
					currentPrefixTableOffset += PrefixDescriptor.BinaryLength;

					if (info.Value is not null)
					{
						var valueDescriptor = new ValueDescriptor(
							currentRelativeValueTableOffset,
							info.Value.Marker
						);

						ValueBufferRawHelpers.WriteUInt32(bspan[currentPrefixTableOffset..], valueDescriptor.Bits);
						currentPrefixTableOffset += ValueDescriptor.BinaryLength;

						info.Value.WriteTo(bspan[currentValueTableOffset..]);
						currentValueTableOffset += info.Value.GetLength();
						currentRelativeValueTableOffset += info.Value.GetLength();
					}

					info.Prefix.AsSpan().WriteTo(bspan[currentPrefixTableOffset..]);
					currentPrefixTableOffset += info.Prefix.Length;
				}

				return new RadixTreeBuffer(buffer);
			}
		}
	}
}
