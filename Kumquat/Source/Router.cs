using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Lime;
using ProtoBuf;

namespace Kumquat
{
	[ProtoContract(SkipConstructor = true)]
	public partial class Router
	{
		#region Vertex

		[ProtoContract(SkipConstructor = true)]
		private class Vertex
		{
			[ProtoMember(1)]
			public string Id;

			[ProtoMember(2)]
            public List<string> Adjacencies = new List<string>();

			public int Distance = -1;
			public Vertex Back = null;

			public Vertex(string id)
			{
				Id = id;
			}
		}

		#endregion

		[ProtoMember(1)]
        Dictionary<string, Vertex> Vertices = new Dictionary<string, Vertex>();
        
		public Router(Dictionary<string, Frame> locations)
		{
			foreach (var path in locations.Keys) {
				var name = Path.GetFileNameWithoutExtension(path);
				Vertices.Add(name, new Vertex(name));
			}

			foreach (var pair in locations) {
				var name = Path.GetFileNameWithoutExtension(pair.Key);
				var frame = pair.Value;
				var vertex = Vertices[name];
				foreach (var exitArea in frame.Descendants<ExitArea>()) {
					var destination = exitArea.ExitTo;
					if (Vertices.Keys.Contains(destination)) {
                        vertex.Adjacencies.Add(destination);
						if (Utils.IsCloseUp(destination)) {
							Vertices[destination].Adjacencies.Add(vertex.Id);
						}
					}

				}
			}
		}

		private List<Vertex> WaveStep(IEnumerable<Vertex> vertices)
		{
			List<Vertex> result = new List<Vertex>();
			foreach (var vertex in vertices) {
				foreach (var name in vertex.Adjacencies) {
                    var v = Vertices[name];
                    if (v.Distance == -1) {
						v.Distance = vertex.Distance + 1;
						v.Back = vertex;
						result.Add(v);
					}
				}
			}
			return result;
		}

		public List<string> Route(string source, string destination)
		{
			foreach (var v in Vertices.Values)
				v.Distance = -1;

			var srcVertex = Vertices[source];
			var dscVertex = Vertices[destination];

			List<Vertex> list = new List<Vertex>() { srcVertex };
			do {
				list = WaveStep(list);
				if (list.Count == 0)
					return null;
			}
			while (dscVertex.Distance == -1);

			List<string> result = new List<string>();
			var vertex = dscVertex;
			do {
				result.Insert(0, vertex.Id);
				vertex = vertex.Back;
			} while (vertex != srcVertex);
			return result;
		}


	}
}
