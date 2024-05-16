namespace Ulang
{
    public struct Version
    {
        public uint Major;
        public uint Minor;
        public uint Revision;

        public Version(uint major, uint minor, uint revision)
        {
            Major = major;
            Minor = minor;
            Revision = revision;
        }
    }
}
