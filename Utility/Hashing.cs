namespace Celeste.Mod.Hyperline
{
    public class Hashing
    {
        public const uint VAL_32_CONST = 0x811c9dc5;
        public const uint PRIME_32_CONST = 0x1000193;

        public static uint FNV1Hash(string str)
        {
            uint hash = VAL_32_CONST;
            foreach (char c in str)
            {
                hash *= PRIME_32_CONST;
                hash ^= c;
            }
            return hash;
        }
    }
}
