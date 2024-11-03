namespace Celeste.Mod.Hyperline
{
    public class Hashing
    {
        public const uint VAL32CONST = 0x811c9dc5;
        public const uint PRIME32CONST = 0x1000193;

        public static uint FNV1Hash(string str)
        {
            uint hash = VAL32CONST;
            foreach (char c in str)
            {
                hash *= PRIME32CONST;
                hash ^= c;
            }
            return hash;
        }
    }
}
