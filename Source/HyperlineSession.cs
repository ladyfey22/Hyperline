namespace Celeste.Mod.Hyperline
{
    using System.IO;

    public class HyperlineSession : EverestModuleBinarySession
    {
        public override void Read(BinaryReader reader) => Hyperline.TriggerManager.Read(reader);
        public override void Write(BinaryWriter writer) => Hyperline.TriggerManager.Write(writer);
    }
}