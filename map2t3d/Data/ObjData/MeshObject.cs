using System;
using System.Collections.Generic;
using System.Text;

namespace map2t3d.Data.ObjData
{
    public class MeshObject
    {
        public string Name { get; set; }
        public List<Face> Faces { get; set; } = new List<Face>();
    }
}
