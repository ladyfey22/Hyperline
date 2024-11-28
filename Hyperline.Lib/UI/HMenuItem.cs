
namespace Celeste.Mod.Hyperline.Lib.UI
{
    public abstract class HMenuItem : TextMenu.Item
    {
        public HMenuItem Parent { get; set; }
        public bool Focused { get; set; }
        public bool ShouldRender { get; set; }

        public virtual void TakeFocus()
        {
            if (Parent != null)
            {
                Parent.Focused = false;
            }

            Focused = true;
        }

        public virtual void LoseFocus()
        {
            if (Parent != null)
            {
                Parent.Focused = true;
            }

            Focused = false;
        }

        public virtual void Exit()
        {
            if (Parent != null)
            {
                Parent.Focused = true;
            }
            else
            {
                Container.Focused = true;
            }
            Focused = false;
            ShouldRender = false;
        }

        public virtual void Enter()
        {
            if (Parent != null)
            {
                Parent.Focused = false;
            }
            else
            {
                Container.Focused = false;
            }
            Focused = true;
            ShouldRender = true;
        }
    }
}
