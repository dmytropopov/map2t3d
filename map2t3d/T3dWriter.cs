using map2t3d.Data.Obj;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Vector3D;

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
            using var reader = new StreamReader(Path.ChangeExtension(_args.FileName, "obj"));

            var brushes = _objReader.Read(reader);
            Console.WriteLine(JsonConvert.SerializeObject(brushes, Formatting.Indented));

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
                    streamWriter.WriteLine("\t\t\tBegin Polygon Flags=1082163200");

                    var uvs = UvConverter.CalculateTextureUV(poly);

                    //var origin = poly.FaceVertices[0].Vertex;
                    var normal = poly.FaceVertices[0].Normal;
                    //var textureU = new double[] { 
                    //    0, // always 0
                    //    +00000.980785, // normalize?
                    //    -00000.195091 // normalize?
                    //};
                    //var textureV = new double[] {
                    //    0, // always 0
                    //    -00000.195091, // normalize?
                    //    -00000.980785 // normalize?
                    //};
                    streamWriter.WriteLine($"\t\t\t\tOrigin   {formatVector(uvs.origin)}");
                    streamWriter.WriteLine($"\t\t\t\tNormal   {formatVector(normal)}");
                    streamWriter.WriteLine($"\t\t\t\tTextureU {formatVector(uvs.textureU)}");
                    streamWriter.WriteLine($"\t\t\t\tTextureV {formatVector(uvs.textureV)}");
                    streamWriter.WriteLine($"\t\t\t\tPan      U=0 V=0");

                    foreach(var vertex in poly.FaceVertices)
                    {
                        streamWriter.WriteLine($"\t\t\t\tVertex   {formatVector(vertex.Vertex)}");
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


        private void WriteDummyBrush(StreamWriter streamWriter)
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

        private string formatFloat(double value)
        {
            return value.ToString(floatFormatString, CultureInfo.InvariantCulture);
        }

        private string formatVector(double[] vector)
        {
            return string.Join(",", vector.Select(s => formatFloat(s)));
        }
    }
}