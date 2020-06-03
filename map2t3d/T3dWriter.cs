using map2t3d.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.IO;
using System.Linq;

namespace map2t3d
{
    public class T3dWriter
    {
        private readonly ILogger _logger;
        private readonly ArgsConfig _args;
        private readonly ObjReader _objReader;

        private const string floatFormatString = "+00000.000000;-00000.000000;+00000.000000";

        public T3dWriter(ILogger<T3dWriter> logger, ObjReader objReader, IOptions<ArgsConfig> options)
        {
            _logger = logger;
            _objReader = objReader;
            _args = options.Value;
        }

        public void Write(StreamWriter streamWriter)
        {
            var brushes = _objReader.Read(Path.ChangeExtension(_args.FileName, "obj"));
            //Console.WriteLine(JsonConvert.SerializeObject(brushes, Formatting.Indented));

            streamWriter.WriteLine("Begin Map");
            streamWriter.WriteLine(@"Begin Actor Class=LevelInfo Name=LevelInfo0
	TimeSeconds=51.827946
	AIProfile(0)=1379
End Actor");

            WriteDummyBrush(streamWriter);

            foreach (var brush in brushes)
            {
                streamWriter.WriteLine($"Begin Actor Class=Brush Name={brush.Name}");
                streamWriter.WriteLine("\tCsgOper=CSG_Add");
                streamWriter.WriteLine("\tBegin Brush Name=Brush");

                streamWriter.WriteLine("\t\tBegin PolyList");

                foreach (var poly in brush.Faces)
                {
                    string textureString = string.IsNullOrEmpty(poly.Material.MaterialName) ? ""
                        : $" Texture={poly.Material.MaterialName.Replace("/", ".", false, CultureInfo.InvariantCulture)}";
                    streamWriter.WriteLine($"\t\t\tBegin Polygon{textureString} Flags=1082163200");

                    var (textureU, textureV, origin) = UvConverter.CalculateTextureUV(poly);

                    var normal = poly.FaceVertices[0].Normal;
                    streamWriter.WriteLine($"\t\t\t\tOrigin   {FormatVector(origin)}");
                    streamWriter.WriteLine($"\t\t\t\tNormal   {FormatVector(normal)}");
                    streamWriter.WriteLine($"\t\t\t\tTextureU {FormatVector(textureU)}");
                    streamWriter.WriteLine($"\t\t\t\tTextureV {FormatVector(textureV)}");
                    streamWriter.WriteLine($"\t\t\t\tPan      U=0 V=0");

                    foreach (var vertex in poly.FaceVertices)
                    {
                        streamWriter.WriteLine($"\t\t\t\tVertex   {FormatVector(vertex.Vertex)}");
                    }

                    streamWriter.WriteLine("\t\t\tEnd Polygon");
                }

                streamWriter.WriteLine("\t\tEnd PolyList");
                streamWriter.WriteLine("\tEnd Brush");
                streamWriter.WriteLine("\tBrush=Model'MyLevel.Brush'");
                streamWriter.WriteLine("End Actor");
            }

            streamWriter.WriteLine("End Map");
        }


        private static void WriteDummyBrush(StreamWriter streamWriter)
        {
            streamWriter.WriteLine($"Begin Actor Class=Brush Name=Brush");
            streamWriter.WriteLine("\tBegin Brush Name=Brush");

            streamWriter.WriteLine(@"Begin PolyList
	Begin Polygon Item=Sheet Flags=264
		Origin   +00128.000000,+00128.000000,+00000.000000
		Normal   +00000.000000,+00000.000000,-00001.000000
		TextureU -00001.000000,+00000.000000,+00000.000000
		TextureV +00000.000000,+00001.000000,+00000.000000
		Pan      U=0 V=0
		Vertex   +00128.000000,+00128.000000,+00000.000000
		Vertex   +00128.000000,-00128.000000,+00000.000000
		Vertex   -00128.000000,-00128.000000,+00000.000000
		Vertex   -00128.000000,+00128.000000,+00000.000000
	End Polygon
End PolyList");

            streamWriter.WriteLine("\tEnd Brush");
            streamWriter.WriteLine("\tBrush=Model'MyLevel.Brush'");
            streamWriter.WriteLine("End Actor");
        }

        private static string FormatFloat(double value)
        {
            return value.ToString(floatFormatString, CultureInfo.InvariantCulture);
        }

        private static string FormatVector(double[] vector)
        {
            return string.Join(",", vector.Select(s => FormatFloat(s)));
        }
    }
}