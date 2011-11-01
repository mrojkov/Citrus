using System;
using System.Collections.Generic;
using System.Text;
using Lime;
using ProtoBuf;

namespace Lime
{
    [ProtoContract]
	[ProtoInclude(101, typeof(SplinePoint))]
    [ProtoInclude(102, typeof(DistortionMeshPoint))]
    public class PointObject : Node
    {
        [ProtoMember(1)]
        public Vector2 Position { get; set; }

        [ProtoMember(2)]
        public SkinningWeights SkinningWeights { get; set; }
    }
}
