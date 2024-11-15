namespace Celeste.Mod.Hyperline
{
    using System.Collections.Generic;

    public class HairTypeManager
    {
        public HairTypeManager()
        {
            hairTypes = [];
        }

        public void AddHairType(IHairType hair)
        {
            uint id = hair.GetHash();
            if (!hairTypes.ContainsKey(id))
            {
                hairTypes[id] = hair;
            }
        }

        public IHairType CreateNewHairType(uint id)
        {
            if (hairTypes.TryGetValue(id, out IHairType value))
            {
                return value.CreateNew();
            }

            return null;
        }

        public IHairType CreateNewHairType(string str)
        {
            uint id = Hashing.FNV1Hash(str);
            return CreateNewHairType(id);
        }

        public IHairType[] GetHairTypes()
        {
            IHairType[] hairTypeList = new IHairType[hairTypes.Count];
            uint index = 0;
            foreach (KeyValuePair<uint, IHairType> hair in hairTypes)
            {
                hairTypeList[index] = hair.Value;
                index++;
            }
            return hairTypeList;
        }

        public int GetHairTypeCount() => hairTypes.Count;

        public Dictionary<uint, IHairType> CopyHairDict() => new(hairTypes);
        public Dictionary<uint, IHairType> CopyHairDict(int dashCount)
        {
            Dictionary<uint, IHairType> returnV = [];
            foreach (KeyValuePair<uint, IHairType> hair in hairTypes)
            {
                returnV[hair.Key] = hair.Value.CreateNew(dashCount);
            }

            return returnV;
        }

        public IHairType GetType(string str) => GetType(Hashing.FNV1Hash(str));

        public IHairType GetType(uint id)
        {
            if (hairTypes.TryGetValue(id, out IHairType value))
            {
                return value;
            }

            return null;
        }

        public KeyValuePair<uint, string>[] GetHairNames()
        {
            KeyValuePair<uint, string>[] hairNames = new KeyValuePair<uint, string>[hairTypes.Count];
            uint i = 0;
            foreach (KeyValuePair<uint, IHairType> hair in hairTypes)
            {
                hairNames[i] = new(hair.Key, Dialog.Clean(hair.Value.GetHairName()));
                i++;
            }
            return hairNames;
        }

        public int InverseIndex(uint hash)
        {
            int i = 0;
            foreach (KeyValuePair<uint, IHairType> hair in hairTypes)
            {
                if (hair.Value.GetHash() == hash)
                {
                    return i;
                }

                i++;
            }
            return -1;
        }

        public bool Has(uint i) => hairTypes.ContainsKey(i);

        private readonly Dictionary<uint, IHairType> hairTypes;
    }
}
