namespace Ulang
{
    public struct Edition
    {
        public uint Year; // First year is 2024
        public uint Month; // 0-12 (Jan=0, Feb=1, Mar=2, etc.)
        public uint Revision;

        public Edition(ushort year, byte month, byte revision)
        {
            Year = year;
            Month = month;
            Revision = revision;
        }
    }
}
