namespace Celeste.Mod.Hyperline
{
    public class Hashing
    {
        public const uint Val32Const = 0x811c9dc5;
        public const uint Prime32Const = 0x1000193;

        public static uint FNV1Hash(string str)
        {
            uint hash = Val32Const;
            foreach (char c in str)
            {
                hash *= Prime32Const;
                hash ^= c;
            }
            return hash;
        }
    }
}
