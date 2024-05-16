using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ulang.Layouts;

namespace Ulang
{
    public class IRModule : IDisposable
    {
        private bool _freed;

        public struct ReadOnlyWrapper<T>
        {
            public delegate T ReadMethod(int index);

            private readonly ReadMethod _r;

            public readonly T this[int index] { get => _r(index); }

            private ReadOnlyWrapper(ReadMethod read)
            {
                _r = read;
            }

            public static ReadOnlyWrapper<T> From(List<T> list)
                => new ReadOnlyWrapper<T>((int i) => list[i]);
            public static ReadOnlyWrapper<T> From(T[] array)
                => new ReadOnlyWrapper<T>((int i) => array[i]);
        }

        internal readonly ProjectFile _project;
        internal readonly List<StructLayout> _structs;
        internal readonly List<UnionLayout> _unions;
        internal readonly List<FieldLayout> _fields;
        internal readonly List<FunctionLayout> _functions;
        internal readonly List<CodeBodyLayout> _bodies;

        private readonly ReadOnlyWrapper<StructLayout> _structWrap;
        private readonly ReadOnlyWrapper<UnionLayout> _unionWrap;
        private readonly ReadOnlyWrapper<FieldLayout> _fieldWrap;
        private readonly ReadOnlyWrapper<FunctionLayout> _functionWrap;
        private readonly ReadOnlyWrapper<CodeBodyLayout> _bodyWrap;

        public ProjectFile Project => _project;
        public ReadOnlyWrapper<StructLayout> Structs => _structWrap;
        public ReadOnlyWrapper<UnionLayout> Unions => _unionWrap;
        public ReadOnlyWrapper<FieldLayout> Globals => _fieldWrap;
        public ReadOnlyWrapper<FunctionLayout> Prototypes => _functionWrap;
        public ReadOnlyWrapper<CodeBodyLayout> Functions => _bodyWrap;

        public bool Write(string path)
        {
            if (!path.EndsWith(".uir", OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                return false;

            // Clear the file of its contents before writing to it
            if (File.Exists(path))
                File.Delete(path);

            using FileStream fs = File.Create(path);
            return Write(fs);
        }

        public bool Write(FileStream stream)
        {
            using BinaryWriter bw = new BinaryWriter(stream);
            return Write(bw);
        }

        public bool Write(BinaryWriter stream)
        {
            if (_bodies == null)
                return false;

            // Header
            stream.Write((byte)0x55);
            stream.Write((byte)0x49);
            stream.Write((byte)0x52);
            stream.Write((byte)0x21);

            // Impl
            stream.Write((ushort)_structs.Count);
            stream.WriteList(_structs);
            stream.Write((ushort)_unions.Count);
            stream.WriteList(_unions);
            stream.Write((ushort)_fields.Count);
            stream.WriteList(_fields);
            stream.Write((ushort)_functions.Count);
            stream.WriteList(_functions);
            stream.Write((ushort)_bodies.Count);
            stream.WriteList(_bodies);

            return true;
        }

        public bool GetStruct(string name, ref StructLayout result)
        {
            int index = _structs.FindIndex(str => str.Name.Equals(name, StringComparison.Ordinal));
            if (index < 0)
                return false;
            result = _structs[index];
            return true;
        }

        public IRModule(ProjectFile proj)
        {
            _project = proj;
            _structs = new();
            _unions = new();
            _fields = new();
            _functions = new();
            _bodies = new();

            _structWrap = ReadOnlyWrapper<StructLayout>.From(_structs);
            _unionWrap = ReadOnlyWrapper<UnionLayout>.From(_unions);
            _fieldWrap = ReadOnlyWrapper<FieldLayout>.From(_fields);
            _functionWrap = ReadOnlyWrapper<FunctionLayout>.From(_functions);
            _bodyWrap = ReadOnlyWrapper<CodeBodyLayout>.From(_bodies);
        }

        private void Dispose(bool disposing)
        {
            if (!_freed)
            {
                // if (disposing)
                // {
                // }

                _structs.Clear();
                _unions.Clear();
                _fields.Clear();
                _functions.Clear();
                _bodies.Clear();

                _freed = true;
            }
        }

        ~IRModule()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
