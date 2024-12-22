using System;

using Barbados.Documents.Serialisation.Values;

namespace Barbados.Documents.Serialisation
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

			public bool PrefixExists(RadixTreePrefix prefix)
			{
				return _root.TryGet(prefix.AsSpan(), out _);
			}

			public RadixTreeBuffer Build()
			{
				return Build(_root);
			}

			public void Reset()
			{
				_root = new RadixTreeNode();
			}

			public Builder AddInt8(RadixTreePrefix name, sbyte value) => AddBuffer(name, ValueBufferFactory.Create(value));
			public Builder AddInt16(RadixTreePrefix name, short value) => AddBuffer(name, ValueBufferFactory.Create(value));
			public Builder AddInt32(RadixTreePrefix name, int value) => AddBuffer(name, ValueBufferFactory.Create(value));
			public Builder AddInt64(RadixTreePrefix name, long value) => AddBuffer(name, ValueBufferFactory.Create(value));
			public Builder AddUInt8(RadixTreePrefix name, byte value) => AddBuffer(name, ValueBufferFactory.Create(value));
			public Builder AddUInt16(RadixTreePrefix name, ushort value) => AddBuffer(name, ValueBufferFactory.Create(value));
			public Builder AddUInt32(RadixTreePrefix name, uint value) => AddBuffer(name, ValueBufferFactory.Create(value));
			public Builder AddUInt64(RadixTreePrefix name, ulong value) => AddBuffer(name, ValueBufferFactory.Create(value));
			public Builder AddFloat32(RadixTreePrefix name, float value) => AddBuffer(name, ValueBufferFactory.Create(value));
			public Builder AddFloat64(RadixTreePrefix name, double value) => AddBuffer(name, ValueBufferFactory.Create(value));
			public Builder AddBoolean(RadixTreePrefix name, bool value) => AddBuffer(name, ValueBufferFactory.Create(value));
			public Builder AddDateTime(RadixTreePrefix name, DateTime value) => AddBuffer(name, ValueBufferFactory.Create(value));
			public Builder AddString(RadixTreePrefix name, string value) => AddBuffer(name, ValueBufferFactory.Create(value));

			public Builder AddInt8Array(RadixTreePrefix name, sbyte[] values) => AddBuffer(name, ValueBufferFactory.Create(values));
			public Builder AddInt16Array(RadixTreePrefix name, short[] values) => AddBuffer(name, ValueBufferFactory.Create(values));
			public Builder AddInt32Array(RadixTreePrefix name, int[] values) => AddBuffer(name, ValueBufferFactory.Create(values));
			public Builder AddInt64Array(RadixTreePrefix name, long[] values) => AddBuffer(name, ValueBufferFactory.Create(values));
			public Builder AddUInt8Array(RadixTreePrefix name, byte[] values) => AddBuffer(name, ValueBufferFactory.Create(values));
			public Builder AddUInt16Array(RadixTreePrefix name, ushort[] values) => AddBuffer(name, ValueBufferFactory.Create(values));
			public Builder AddUInt32Array(RadixTreePrefix name, uint[] values) => AddBuffer(name, ValueBufferFactory.Create(values));
			public Builder AddUInt64Array(RadixTreePrefix name, ulong[] values) => AddBuffer(name, ValueBufferFactory.Create(values));
			public Builder AddFloat32Array(RadixTreePrefix name, float[] values) => AddBuffer(name, ValueBufferFactory.Create(values));
			public Builder AddFloat64Array(RadixTreePrefix name, double[] values) => AddBuffer(name, ValueBufferFactory.Create(values));
			public Builder AddBooleanArray(RadixTreePrefix name, bool[] values) => AddBuffer(name, ValueBufferFactory.Create(values));
			public Builder AddDateTimeArray(RadixTreePrefix name, DateTime[] values) => AddBuffer(name, ValueBufferFactory.Create(values));
			public Builder AddStringArray(RadixTreePrefix name, string[] values) => AddBuffer(name, ValueBufferFactory.Create(values));

			public Builder AddBuffer(RadixTreePrefix name, IValueBuffer buffer)
			{
				return AddBuffer(name.AsSpan(), buffer);
			}

			public Builder AddBuffer(RadixTreePrefixSpan nameSpan, IValueBuffer buffer)
			{
				_root.Add(nameSpan, buffer);
				return this;
			}

			public Builder AddPrefix(RadixTreePrefix name)
			{
				_root.Add(name.AsSpan(), null);
				return this;
			}
		}
	}
}
