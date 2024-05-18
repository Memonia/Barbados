using System;

using Barbados.StorageEngine.Documents.Binary.ValueBuffers;

namespace Barbados.StorageEngine.Documents.Binary
{
	internal partial class ObjectBuffer
	{
		public sealed partial class Builder
		{
			private readonly Accumulator _accumulator;

			public Builder()
			{
				_accumulator = new();
			}

			public ObjectBuffer Build()
			{
				return Build(_accumulator);
			}

			public void Reset()
			{
				_accumulator.Clear();
			}

			public Builder AddInt8(string name, sbyte value) => AddBuffer(name, new ValueInt8Buffer(value));
			public Builder AddInt16(string name, short value) => AddBuffer(name, new ValueInt16Buffer(value));
			public Builder AddInt32(string name, int value) => AddBuffer(name, new ValueInt32Buffer(value));
			public Builder AddInt64(string name, long value) => AddBuffer(name, new ValueInt64Buffer(value));
			public Builder AddUInt8(string name, byte value) => AddBuffer(name, new ValueUInt8Buffer(value));
			public Builder AddUInt16(string name, ushort value) => AddBuffer(name, new ValueUInt16Buffer(value));
			public Builder AddUInt32(string name, uint value) => AddBuffer(name, new ValueUInt32Buffer(value));
			public Builder AddUInt64(string name, ulong value) => AddBuffer(name, new ValueUInt64Buffer(value));
			public Builder AddFloat32(string name, float value) => AddBuffer(name, new ValueFloat32Buffer(value));
			public Builder AddFloat64(string name, double value) => AddBuffer(name, new ValueFloat64Buffer(value));
			public Builder AddBoolean(string name, bool value) => AddBuffer(name, new ValueBooleanBuffer(value));
			public Builder AddDateTime(string name, DateTime value) => AddBuffer(name, new ValueDateTimeBuffer(value));
			public Builder AddString(string name, string value) => AddBuffer(name, new ValueStringBuffer(value));

			public Builder AddInt8Array(string name, sbyte[] values) => AddBuffer(name, new ValueInt8BufferArray(values));
			public Builder AddInt16Array(string name, short[] values) => AddBuffer(name, new ValueInt16BufferArray(values));
			public Builder AddInt32Array(string name, int[] values) => AddBuffer(name, new ValueInt32BufferArray(values));
			public Builder AddInt64Array(string name, long[] values) => AddBuffer(name, new ValueInt64BufferArray(values));
			public Builder AddUInt8Array(string name, byte[] values) => AddBuffer(name, new ValueUInt8BufferArray(values));
			public Builder AddUInt16Array(string name, ushort[] values) => AddBuffer(name, new ValueUInt16BufferArray(values));
			public Builder AddUInt32Array(string name, uint[] values) => AddBuffer(name, new ValueUInt32BufferArray(values));
			public Builder AddUInt64Array(string name, ulong[] values) => AddBuffer(name, new ValueUInt64BufferArray(values));
			public Builder AddFloat32Array(string name, float[] values) => AddBuffer(name, new ValueFloat32BufferArray(values));
			public Builder AddFloat64Array(string name, double[] values) => AddBuffer(name, new ValueFloat64BufferArray(values));
			public Builder AddBooleanArray(string name, bool[] values) => AddBuffer(name, new ValueBooleanBufferArray(values));
			public Builder AddDateTimeArray(string name, DateTime[] values) => AddBuffer(name, new ValueDateTimeBufferArray(values));
			public Builder AddStringArray(string name, string[] values) => AddBuffer(name, new ValueStringBufferArray(values));

			public Builder AddBuffer(string name, IValueBuffer buffer)
			{
				_accumulator.Add(new ValueStringBuffer(name), buffer);
				return this;
			}
		}
	}
}
