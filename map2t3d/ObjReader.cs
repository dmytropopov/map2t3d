using map2t3d.Config;
using map2t3d.Data.ObjData;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace map2t3d
{
    public class ObjReader
    {
        private readonly ILogger<ObjReader> _logger;
        private readonly FoldersOptions _foldersOptions;
        private readonly TexturesConversionOptions _textureConversionOptions;
        private readonly List<double[]> vertices = new List<double[]>();
        private readonly List<double[]> normals = new List<double[]>();
        private readonly List<double[]> uvs = new List<double[]>();
        private readonly IDictionary<string, MaterialInfo> materials = new Dictionary<string, MaterialInfo>();

        private string _workFolder;

        public ObjReader(ILogger<ObjReader> logger, IOptions<TexturesConversionOptions> textureConversionOptions, IOptions<FoldersOptions> foldersOptions)
        {
            _logger = logger;
            _foldersOptions = foldersOptions.Value;
            _textureConversionOptions = textureConversionOptions.Value;
        }

        public List<MeshObject> Read(string fileName)
        {
            using var reader = new StreamReader(fileName);
            _workFolder = Path.GetDirectoryName(fileName);

            List<MeshObject> list = new List<MeshObject>();

            string line;
            int lineNumber = 0;
            var currentObject = new MeshObject();
            MaterialInfo currentMaterial = null;
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
                        currentObject = new MeshObject()
                        {
                            Name = words[1]
                        };
                        list.Add(currentObject);
                        currentMaterial = null;
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
                            Material = currentMaterial,
                            FaceVertices = faces.Select(s => new FaceVertex
                            {
                                Vertex = vertices[s.vertexIndex],
                                Normal = normals[s.normalIndex],
                                Uv = uvs[s.uvIndex]
                            }).ToList()
                        };

                        // reverse clockwiseness of the vertices after normal flip
                        var reversedTail = face.FaceVertices.Skip(1).Reverse();
                        var reversedAll = new List<FaceVertex>
                        {
                            face.FaceVertices[0]
                        };
                        reversedAll.AddRange(reversedTail);
                        face.FaceVertices = reversedAll;

                        currentObject.Faces.Add(face);
                        break;
                    case "MTLLIB":
                        LoadMaterialLibrary(words[1]);
                        break;
                    case "USEMTL":
                        currentMaterial = materials[words[1]];
                        break;
                    default:
                        _logger.LogInformation("Line {line}. Unknown verb: {verb}", lineNumber, verb);
                        break;
                }

                lineNumber++;
            }

            DisplayTexturePackages();

            return list;
        }

        private void DisplayTexturePackages()
        {
            Console.WriteLine("Apparently, following texture packages are used. Please load them before import in UnrealEd:");
            int i = 1;
            foreach (var packageName in materials.Keys.Select(s => s.Split("/")[0]).Distinct())
            {
                Console.WriteLine($"   {i++}. {packageName}");
            }
        }

        private void LoadMaterialLibrary(string materialLibraryFileName)
        {
            using var reader = new StreamReader(Path.Combine(_workFolder, materialLibraryFileName));

            string line;
            int lineNumber = 0;
            while ((line = reader.ReadLine()) != null)
            {
                var words = line.Trim().Split(" ");
                var verb = words[0].ToUpperInvariant();
                switch (verb)
                {
                    case "NEWMTL":
                        var newMaterial = new MaterialInfo
                        {
                            MaterialName = words[1],
                            TextureSizeU = _textureConversionOptions.DefaultTextureSize,
                            TextureSizeV = _textureConversionOptions.DefaultTextureSize
                        };
                        materials.Add(words[1], newMaterial);
                        ReadTextureSize(words[1], newMaterial);
                        break;
                    default:
                        _logger.LogInformation("Material Library '{materialLibraryFileName}' Line {line}. Unknown verb: {verb}", materialLibraryFileName, lineNumber, verb);
                        break;
                }

                lineNumber++;
            }
        }

        private void ReadTextureSize(string v, MaterialInfo material)
        {
            if (string.IsNullOrEmpty(_foldersOptions?.TexturesFolder))
            {
                return;
            }

            try
            {
                var textureNameParts = material.MaterialName.Split("/");
                var textureFolder = Path.Combine(_foldersOptions.TexturesFolder, textureNameParts[0]);
                var textureFileName = textureNameParts[1] + ".*";
                var fileNameFound = Directory.GetFiles(textureFolder, textureFileName).FirstOrDefault();

                if (fileNameFound != default)
                {
                    using var image = Image.FromFile(fileNameFound);
                    material.TextureSizeU = image.Width;
                    material.TextureSizeV = image.Height;
                }
            }
            catch (IOException ex)
            {
                _logger.LogWarning(ex, "Error reading texture: {materialName}", material.MaterialName);
            }
        }
    }
}