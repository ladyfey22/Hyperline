using System.Collections.Generic;

namespace Celeste.Mod.Hyperline
{
    public class HairTypeManager
    {
        public HairTypeManager()
        {
            hairTypes = new Dictionary<uint, IHairType>();
        }

        public void AddHairType(IHairType hair)
        {
            uint id = hair.GetHash();
            if (!hairTypes.ContainsKey(id))
                hairTypes[id] = hair;
        }

        public IHairType CreateNewHairType(uint id)
        {
            if (hairTypes.ContainsKey(id))
                return hairTypes[id].CreateNew();
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

        public int GetHairTypeCount()
        {
            return hairTypes.Count;
        }

        public Dictionary<uint, IHairType> CopyHairDict()
        {
            return new Dictionary<uint, IHairType>(hairTypes);
        }

        public Dictionary<uint, IHairType> CopyHairDict(int dashCount)
        {
            Dictionary<uint, IHairType> returnV = new Dictionary<uint, IHairType>();
            foreach (KeyValuePair<uint, IHairType> hair in hairTypes)
                returnV[hair.Key] = hair.Value.CreateNew(dashCount);
            return returnV;
        }

        public IHairType GetType(uint id)
        {
            if (hairTypes.ContainsKey(id))
                return hairTypes[id];
            return null;
        }

        public KeyValuePair<uint, string>[] GetHairNames()
        {
            KeyValuePair<uint, string>[] hairNames = new KeyValuePair<uint, string>[hairTypes.Count];
            uint i = 0;
            foreach (KeyValuePair<uint, IHairType> hair in hairTypes)
            {
                hairNames[i] = new KeyValuePair<uint, string>(hair.Key, Dialog.Clean(hair.Value.GetHairName()));
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
                    return i;
                i++;
            }
            return -1;
        }

        public bool Has(uint i)
        {
            return hairTypes.ContainsKey(i);
        }

        private Dictionary<uint, IHairType> hairTypes;
    }
}
