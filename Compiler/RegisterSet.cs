using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Ulang.Layouts;

namespace Ulang
{
    public abstract class RegisterSet<T, DataT>
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>, IConvertible
        where DataT : struct
    {
        public static readonly int Size = T.MaxValue.ToInt32(null);

        private unsafe static T IntToT(int value)
        {
            T result = default;

            if (sizeof(T) == 1)
            {
                Unsafe.As<T, byte>(ref result) = (byte)value;
            } else if (sizeof(T) == 2)
            {
                Unsafe.As<T, ushort>(ref result) = (ushort)value;
            } else if (sizeof(T) == 4)
            {
                Unsafe.As<T, uint>(ref result) = (uint)value;
            } else
                throw new InvalidOperationException();

            return result;
        }

        private static DataT rZero = default;

        private readonly DataT[] registers = new DataT[Size];
        private T mostRecent;
        private int count;

        protected abstract ref T GetNextOf(ref DataT register);
        protected abstract ref string GetNameOf(ref DataT register);
        protected abstract ref TypeLayout GetTypeOf(ref DataT register);

        public T Search(string name)
        {
            for (int i = 1; i < registers.Length; ++i)
                if (name.Equals(GetNameOf(ref registers[i]), StringComparison.Ordinal))
                {
                    return IntToT(i);
                }

            rZero = default;
            return T.Zero;
        }

        public T RequestNew(ref DataT register)
        {
            if (mostRecent == T.MaxValue)
            {
                rZero = default;
                register = ref rZero;
                return T.Zero;
            }

            if (mostRecent == T.Zero)
            {
                ++mostRecent;
                register = ref registers[1];
                return T.One;
            }
            
            T next = GetNextOf(ref GetRegister(mostRecent));
            register = ref registers[next.ToInt32(null)];
            GetNextOf(ref registers[next.ToInt32(null)]) = IntToT(++count);
            return ++mostRecent;
        }

        public void FreeRegister(T index)
        {
            Debug.Assert(index != T.Zero);
            Debug.Assert(count > 0);

            --count;
            GetNextOf(ref registers[count]) = index;
            GetRegister(index) = default;
        }


        public ref DataT GetRegister(T index)
            => ref registers[index.ToInt32(null)];
        public ref string GetName(T index)
            => ref GetNameOf(ref GetRegister(index));
        public ref TypeLayout GetType(T index)
            => ref GetTypeOf(ref GetRegister(index));

        public void Clear()
        {
            registers.AsSpan().Clear();
        }
    }

    public sealed class PrimitiveRegisterSet<T> : RegisterSet<T, PrimitiveRegisterSet<T>.Data>
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>, IConvertible
    {
        public struct Data
        {
            public T Next;
            public string Name;
            public TypeLayout Type;
            public Int128 Value;
        }

        protected override ref T GetNextOf(ref Data register)
            => ref register.Next;
        protected override ref string GetNameOf(ref Data register)
            => ref register.Name;
        protected override ref TypeLayout GetTypeOf(ref Data register)
            => ref register.Type;
        public ref Int128 GetValue(T index)
            => ref GetRegister(index).Value;
    }

    public static class AggregateRegisterSetTypes
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct StructValue
        {
            public Dictionary<string, DataValue> Fields;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct PrimitiveValue
        {
            private readonly object Padding;
            public Int128 Value;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct VectorValue
        {
            public DataValue[] Elements;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct DataValue
        {
            [FieldOffset(0)]
            public StructValue Struct;
            [FieldOffset(0)]
            public PrimitiveValue Primitive;
            [FieldOffset(0)]
            public VectorValue Vector;
        }
    }

    public sealed class AggregateRegisterSet<T> : RegisterSet<T, AggregateRegisterSet<T>.Data>
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>, IConvertible
    {
        public struct Data
        {
            public T Next;
            public string Name;
            public TypeLayout Type;
            public AggregateRegisterSetTypes.DataValue Value;
        }

        protected override ref T GetNextOf(ref Data register)
            => ref register.Next;
        protected override ref string GetNameOf(ref Data register)
            => ref register.Name;
        protected override ref TypeLayout GetTypeOf(ref Data register)
            => ref register.Type;
        public ref AggregateRegisterSetTypes.DataValue GetValue(T index)
            => ref GetRegister(index).Value;
    }

    public class RegisterSim : IDisposable
    {
        public PrimitiveRegisterSet<byte> Primitive { get; } = new();
        public AggregateRegisterSet<ushort> Aggregate { get; } = new();

        public void Dispose()
        {
            Primitive.Clear();
            Aggregate.Clear();
            GC.SuppressFinalize(this);
        }
    }
}
