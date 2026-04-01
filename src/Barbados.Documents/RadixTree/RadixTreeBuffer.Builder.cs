using System;

using Barbados.Documents.RadixTree.Values;

namespace Barbados.Documents.RadixTree
{
	internal partial class RadixTreeBuffer
	{
		public sealed partial class Builder
		{
			public static RadixTreeBuffer EmptyBuffer { get; } = Build(new RadixTreeNode());

			private RadixTreeNode _root;

			public Builder()
			{
				_root = new RadixTreeNode();
			}

			public bool PrefixExists(RadixTreePrefixSpan prefix)
			{
				return _root.TryGet(prefix, out _);
			}

			public bool PrefixHasValue(RadixTreePrefixSpan prefix)
			{
				return _root.TryGet(prefix, out var value) && value is not null;
			}

			public RadixTreeBuffer Build()
			{
				return Build(_root);
			}

			public void Reset()
			{
				_root = new RadixTreeNode();
			}

			public Builder AddInt8(RadixTreePrefixSpan prefix, sbyte value) => AddBuffer(prefix, ValueBufferFactory.Create(value));
			public Builder AddInt16(RadixTreePrefixSpan prefix, short value) => AddBuffer(prefix, ValueBufferFactory.Create(value));
			public Builder AddInt32(RadixTreePrefixSpan prefix, int value) => AddBuffer(prefix, ValueBufferFactory.Create(value));
			public Builder AddInt64(RadixTreePrefixSpan prefix, long value) => AddBuffer(prefix, ValueBufferFactory.Create(value));
			public Builder AddUInt8(RadixTreePrefixSpan prefix, byte value) => AddBuffer(prefix, ValueBufferFactory.Create(value));
			public Builder AddUInt16(RadixTreePrefixSpan prefix, ushort value) => AddBuffer(prefix, ValueBufferFactory.Create(value));
			public Builder AddUInt32(RadixTreePrefixSpan prefix, uint value) => AddBuffer(prefix, ValueBufferFactory.Create(value));
			public Builder AddUInt64(RadixTreePrefixSpan prefix, ulong value) => AddBuffer(prefix, ValueBufferFactory.Create(value));
			public Builder AddFloat32(RadixTreePrefixSpan prefix, float value) => AddBuffer(prefix, ValueBufferFactory.Create(value));
			public Builder AddFloat64(RadixTreePrefixSpan prefix, double value) => AddBuffer(prefix, ValueBufferFactory.Create(value));
			public Builder AddBoolean(RadixTreePrefixSpan prefix, bool value) => AddBuffer(prefix, ValueBufferFactory.Create(value));
			public Builder AddDateTime(RadixTreePrefixSpan prefix, DateTime value) => AddBuffer(prefix, ValueBufferFactory.Create(value));
			public Builder AddString(RadixTreePrefixSpan prefix, string value) => AddBuffer(prefix, ValueBufferFactory.Create(value));

			public Builder AddInt8Array(RadixTreePrefixSpan prefix, sbyte[] values) => AddBuffer(prefix, ValueBufferFactory.Create(values));
			public Builder AddInt16Array(RadixTreePrefixSpan prefix, short[] values) => AddBuffer(prefix, ValueBufferFactory.Create(values));
			public Builder AddInt32Array(RadixTreePrefixSpan prefix, int[] values) => AddBuffer(prefix, ValueBufferFactory.Create(values));
			public Builder AddInt64Array(RadixTreePrefixSpan prefix, long[] values) => AddBuffer(prefix, ValueBufferFactory.Create(values));
			public Builder AddUInt8Array(RadixTreePrefixSpan prefix, byte[] values) => AddBuffer(prefix, ValueBufferFactory.Create(values));
			public Builder AddUInt16Array(RadixTreePrefixSpan prefix, ushort[] values) => AddBuffer(prefix, ValueBufferFactory.Create(values));
			public Builder AddUInt32Array(RadixTreePrefixSpan prefix, uint[] values) => AddBuffer(prefix, ValueBufferFactory.Create(values));
			public Builder AddUInt64Array(RadixTreePrefixSpan prefix, ulong[] values) => AddBuffer(prefix, ValueBufferFactory.Create(values));
			public Builder AddFloat32Array(RadixTreePrefixSpan prefix, float[] values) => AddBuffer(prefix, ValueBufferFactory.Create(values));
			public Builder AddFloat64Array(RadixTreePrefixSpan prefix, double[] values) => AddBuffer(prefix, ValueBufferFactory.Create(values));
			public Builder AddBooleanArray(RadixTreePrefixSpan prefix, bool[] values) => AddBuffer(prefix, ValueBufferFactory.Create(values));
			public Builder AddDateTimeArray(RadixTreePrefixSpan prefix, DateTime[] values) => AddBuffer(prefix, ValueBufferFactory.Create(values));
			public Builder AddStringArray(RadixTreePrefixSpan prefix, string[] values) => AddBuffer(prefix, ValueBufferFactory.Create(values));

			public Builder AddBuffer(RadixTreePrefixSpan prefix, IValueBuffer buffer)
			{
				_root.Add(prefix, buffer);
				return this;
			}

			public Builder AddPrefix(RadixTreePrefixSpan prefix)
			{
				_root.Add(prefix, null);
				return this;
			}
		}
	}
}
