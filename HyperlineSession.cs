using System.IO;

namespace Celeste.Mod.Hyperline
{
    public class HyperlineSession : EverestModuleBinarySession
    {

        public override void Read(BinaryReader reader)
        {
            Hyperline.Instance.triggerManager.Read(reader);
        }

        public override void Write(BinaryWriter writer)
        {
            Hyperline.Instance.triggerManager.Write(writer);
        }
    }
}
