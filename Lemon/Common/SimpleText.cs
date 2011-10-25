using Lime;
using ProtoBuf;

namespace Lemon
{
    [ProtoContract]
    public class SimpleText : Widget
    {
        [ProtoMember(1)]
        public PersistentFont Font = new PersistentFont();

        [ProtoMember(2)]
        public string Text;

        [ProtoMember(3)]
        public float FontHeight = 15;

        [ProtoMember(4)]
        public float Spacing = 0;

        [ProtoMember(5)]
        public HorizontalAlign HorizontalAlign;

        [ProtoMember(6)]
        public VerticalAlign VerticalAlign;

        public SimpleText () { }

        public override void Render()
        {
            Renderer.Instance.WorldMatrix = WorldMatrix;
            Renderer.Instance.Blending = WorldBlending;
            if (!string.IsNullOrEmpty(Text))
                Renderer.Instance.DrawTextLine (Font.Instance, Vector2.Zero, Text, WorldColor, FontHeight);
        }
    }
}
