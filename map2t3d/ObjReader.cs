using map2t3d.Data.Obj;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace map2t3d
{
    public class ObjReader
    {
        private readonly ILogger<ObjReader> _logger;

        private List<double[]> vertices = new List<double[]>();
        private List<double[]> normals = new List<double[]>();
        private List<double[]> uvs = new List<double[]>();

        public ObjReader(ILogger<ObjReader> logger)
        {
            _logger = logger;
        }

        public List<Obj> Read(StreamReader reader)
        {
            List<Obj> list = new List<Obj>();

            string line;
            int lineNumber = 0;
            var currentObject = new Obj();
            while ((line = reader.ReadLine()) != null)
            {
                var words = line.Trim().Split(" ");
                var verb = words[0].ToUpperInvariant();
                switch (verb)
                {
                    case "":
                        break;
                    case "#":
                        break;
                    case "V":
                        vertices.Add(new double[] {
                            double.Parse(words[1], CultureInfo.InvariantCulture), // swap Y and Z
                            double.Parse(words[3], CultureInfo.InvariantCulture),
                            double.Parse(words[2], CultureInfo.InvariantCulture)
                        });
                        break;
                    case "VN":
                        normals.Add(new double[] {
                            double.Parse(words[1], CultureInfo.InvariantCulture), // swap Y and Z
                            double.Parse(words[3], CultureInfo.InvariantCulture),
                            double.Parse(words[2], CultureInfo.InvariantCulture)
                        });
                        break;
                    case "VT":
                        uvs.Add(new double[] {
                            double.Parse(words[1], CultureInfo.InvariantCulture),
                            double.Parse(words[2], CultureInfo.InvariantCulture)
                        });
                        break;
                    case "O":
                        currentObject = new Obj()
                        {
                            Name = words[1]
                        };
                        list.Add(currentObject);
                        break;
                    case "F":
                        var faces = words.Skip(1)
                            .Select(faceVertexString =>
                            {
                                var faceIndices = faceVertexString.Split("/");
                                return new
                                {
                                    vertexIndex = int.Parse(faceIndices[0], CultureInfo.InvariantCulture) - 1,
                                    uvIndex = int.Parse(faceIndices[1], CultureInfo.InvariantCulture) - 1,
                                    normalIndex = int.Parse(faceIndices[2], CultureInfo.InvariantCulture) - 1
                                };
                            });
                        if (faces.Any(f => f.normalIndex != faces.First().normalIndex))
                        {
                            _logger.LogWarning("Different normals in line {line}", lineNumber);
                        }
                        var face = new Face()
                        {
                            TextureName = "dummy",
                            FaceVertices = faces.Select(s => new FaceVertex
                            {
                                Vertex = vertices[s.vertexIndex],
                                Normal = normals[s.normalIndex],
                                Uv = uvs[s.uvIndex]
                            }).ToList()
                        };

                        // reverse clockwiseness of the vertices after normal flip
                        var reversedTail = face.FaceVertices.Skip(1).Reverse();
                        var reversedAll = new List<FaceVertex>();
                        reversedAll.Add(face.FaceVertices[0]);
                        reversedAll.AddRange(reversedTail);
                        face.FaceVertices = reversedAll;

                        currentObject.Faces.Add(face);
                        break;
                    default:
                        _logger.LogWarning("Line {line}. Unknown verb: {verb}", lineNumber, verb);
                        break;
                }

                lineNumber++;
            }

            return list;
        }
    }
}