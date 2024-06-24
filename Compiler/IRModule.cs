using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Ulang.Layouts;

namespace Ulang
{
    public class IRModule : IDisposable
    {
        private static readonly byte[] _magic = [0x55, 0x49, 0x52, 0x21];
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

        public bool Write(string path, bool debug = false)
        {
            if (!path.EndsWith(".uir", OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                return false;

            // Clear the file of its contents before writing to it
            if (File.Exists(path))
                File.Delete(path);

            using FileStream fs = File.Open(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
            return Write(fs, debug);
        }

        public bool Write(FileStream stream, bool debug = false)
        {
            using BinaryWriter bw = new BinaryWriter(stream);
            return Write(bw, debug);
        }

        public bool Write(BinaryWriter stream, bool debug = false)
        {
            if (_bodies == null)
                return false;

            // Header
            stream.Write(_magic);

            if (debug)
            {
                stream.Write(true);
                stream.WriteUtf8(_project.Id);
                stream.WriteUtf8(_project.Name);
                stream.Write(_project.Version.Major);
                stream.Write(_project.Version.Minor);
                stream.Write(_project.Version.Revision);
            } else
            {
                stream.Write(false);
            }

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

        public static bool Read(string path, out IRModule module)
        {
            Unsafe.SkipInit(out module);

            if (!path.EndsWith(".uir", OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                return false;

            if (!File.Exists(path))
                return false;

            using FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Read(fs, out module);
        }

        public static bool Read(FileStream stream, out IRModule module)
        {
            Unsafe.SkipInit(out module);

            using BinaryReader br = new BinaryReader(stream);
            return Read(br, out module);
        }

        public static bool Read(BinaryReader stream, out IRModule module)
        {
            Unsafe.SkipInit(out module);

            // Header
            {
                Span<byte> magic = stackalloc byte[4];
                if (stream.Read(magic) < 4 || !magic.SequenceEqual(_magic))
                    return false;
            }

            ProjectFile proj = new ProjectFile();

            {
                int strippedFlag = stream.BaseStream.ReadByte();

                if (strippedFlag == -1)
                    return false;

                else if (strippedFlag != 0)
                {
                    proj.Id = stream.ReadUtf8();
                    proj.Name = stream.ReadUtf8();
                    proj.Version.Major = stream.ReadUInt32();
                    proj.Version.Minor = stream.ReadUInt32();
                    proj.Version.Revision = stream.ReadUInt32();
                }
            }

            module = new IRModule(proj);

            // Impl
            module._structs.Capacity = stream.ReadUInt16();
            stream.ReadList(module._structs);
            module._unions.Capacity = stream.ReadUInt16();
            stream.ReadList(module._unions);
            module._fields.Capacity = stream.ReadUInt16();
            stream.ReadList(module._fields);
            module._functions.Capacity = stream.ReadUInt16();
            stream.ReadList(module._functions);
            module._bodies.Capacity = stream.ReadUInt16();
            stream.ReadList(module._bodies);

            return true;
        }

        public bool TryGetStruct(string name, ref StructLayout result)
        {
            int index = _structs.FindIndex(str => str.Name.Equals(name, StringComparison.Ordinal));
            if (index < 0)
                return false;
            result = _structs[index];
            return true;
        }
        public bool TryGetUnion(string name, ref UnionLayout result)
        {
            int index = _unions.FindIndex(str => str.Name.Equals(name, StringComparison.Ordinal));
            if (index < 0)
                return false;
            result = _unions[index];
            return true;
        }
        public bool TryGetGlobal(string name, ref FieldLayout result)
        {
            int index = _fields.FindIndex(str => str.Name.Equals(name, StringComparison.Ordinal));
            if (index < 0)
                return false;
            result = _fields[index];
            return true;
        }
        public bool TryGetFunction(string name, ref FunctionLayout result)
        {
            int index = _functions.FindIndex(str => str.Name.Equals(name, StringComparison.Ordinal));
            if (index < 0)
                return false;
            result = _functions[index];
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
