using map2t3d.Data.ObjData;
using System;
using System.Collections.Generic;
using Vector3D;
using System.Text;

namespace map2t3d
{
    /// <summary>
    /// Algorithm borrowed from ASE2T3D sources
    /// </summary>
    public static class UvConverter
    {
        private const double MINIMUM_DIVISOR = 0.00000000001;

        public static (double[] textureU, double[] textureV, double[] origin) CalculateTextureUV(
            Face poly, 
            double textureWidth = 256, 
            double textureHeight = 256, 
            double textureUTiling = 1,
            double textureVTiling = 1)
        {
            var textureU = new double[3];
            var textureV = new double[3];
            var origin = new double[3];

            //double texture_width = 256;
            //double texture_height = 256;

            //
            // Polygon data
            //

            //
            // Texture Coords.
            //
            //double dux = 0.0, duy =0.0, duz =0.0, dvx = 0.0, dvy = 0.0, dvz = 0.0;

            /*
            From the ASE - DXA file

            dScaleOU = -0.5 - dUF/2;
            dScaleVO = -0.5 - dVF/2;

            dt = (dUF * u));

            pFile << "\t" << ( -dUO * dUF + dScaleOU + dt ) << "\t";

            dt = (dVF * v);

            pFile <<  -( -dVO * dVF + dScaleVO + dt -1) << "\n"; 
            */

            var dUF = textureUTiling;
            var dVF = textureVTiling;
            const double dUO = 0;
            const double dVO = 0;

            double dScaleUO = -0.5 - (dUF / 2.0);
            double dScaleVO = -0.5 - (dVF / 2.0);

            //double ds0 = ase.geomobjects.at(i).mesh.tvert_list[ase.geomobjects.at(i).mesh.tface_list.at(v + 0)].x;
            //double dt0 = ase.geomobjects.at(i).mesh.tvert_list[ase.geomobjects.at(i).mesh.tface_list.at(v + 0)].y;
            double ds0 = poly.FaceVertices[0].Uv[0];
            double dt0 = poly.FaceVertices[0].Uv[1];

            double ds1 = poly.FaceVertices[1].Uv[0];
            double dt1 = poly.FaceVertices[1].Uv[1];

            double ds2 = poly.FaceVertices[2].Uv[0];
            double dt2 = poly.FaceVertices[2].Uv[1];

            //
            // Scale
            //
            ds0 *= dUF;
            dt0 *= dVF;

            ds1 *= dUF;
            dt1 *= dVF;

            ds2 *= dUF;
            dt2 *= dVF;

            ds0 = (-dUO * dUF) + dScaleUO + ds0;        //  OpenGL / DirectX tex coords of first vertex
            dt0 = -((-dVO * dVF) + dScaleVO + dt0 - 1.0);

            ds1 = (-dUO * dUF) + dScaleUO + ds1;        //  OpenGL / DirectX tex coords of second vertex
            dt1 = -((-dVO * dVF) + dScaleVO + dt1 - 1.0);

            ds2 = (-dUO * dUF) + dScaleUO + ds2;        //  OpenGL / DirectX tex coords of third vertex
            dt2 = -((-dVO * dVF) + dScaleVO + dt2 - 1.0);

            //
            // Translate so that coord one is minimum possible
            //
            int translateU = (int)ds0;
            int translateV = (int)dt0;

            double ftranslateU = (double)translateU;
            double ftranslateV = (double)translateV;

            ds0 -= ftranslateU;
            ds1 -= ftranslateU;
            ds2 -= ftranslateU;

            dt0 -= ftranslateV;
            dt1 -= ftranslateV;
            dt2 -= ftranslateV;

            // 
            // Coords
            //
            Vector pt0 = new Vector(poly.FaceVertices[0].Vertex[0], -poly.FaceVertices[0].Vertex[1], poly.FaceVertices[0].Vertex[2]);
            Vector pt1 = new Vector(poly.FaceVertices[1].Vertex[0], -poly.FaceVertices[1].Vertex[1], poly.FaceVertices[1].Vertex[2]);
            Vector pt2 = new Vector(poly.FaceVertices[2].Vertex[0], -poly.FaceVertices[2].Vertex[1], poly.FaceVertices[2].Vertex[2]);
            //ASE::GeomPoint pt0 = ase.geomobjects.at(i).mesh.vertex_list[ase.geomobjects.at(i).mesh.face_list[v + 0]];
            //ASE::GeomPoint pt1 = ase.geomobjects.at(i).mesh.vertex_list[ase.geomobjects.at(i).mesh.face_list[v + 1]];
            //ASE::GeomPoint pt2 = ase.geomobjects.at(i).mesh.vertex_list[ase.geomobjects.at(i).mesh.face_list[v + 2]];

            /*
            fn getTextureGrad pt0 pt1 pt2 val0 val1 val2 = (
            dpt1 = pt1 - pt0
            dpt2 = pt2 - pt0

            dv1 = val1 - val0
            dv2 = val2 - val0
            */
            Vector dpt1 = Vector.VectorDiff(pt1, pt0);
            Vector dpt2 = Vector.VectorDiff(pt2, pt0);

            Vector dv1 = new Vector(ds1 - ds0, dt1 - dt0, 0.0);
            Vector dv2 = new Vector(ds2 - ds0, dt2 - dt0, 0.0);
            //ASE::GeomPoint dv1(ds1-ds0, dt1 - dt0, 0.0 );
            //ASE::GeomPoint dv2(ds2-ds0, dt2 - dt0, 0.0 );

            /*
            -- Compute the 2D matrix values, and invert the matrix.
            dpt11 = dot dpt1 dpt1
            dpt12 = dot dpt1 dpt2
            dpt22 = dot dpt2 dpt2
            factor = 1.0 / (dpt11 * dpt22 - dpt12 * dpt12)
            */
            double dpt11 = Vector.VectorDot(dpt1, dpt1);
            double dpt12 = Vector.VectorDot(dpt1, dpt2);
            double dpt22 = Vector.VectorDot(dpt2, dpt2);
            //assert((dpt11 * dpt22 - dpt12 * dpt12));

            var assert = dpt11 * dpt22 - dpt12 * dpt12;
            if (assert == 0)
            {
                throw new Exception("dpt11 * dpt22 - dpt12 * dpt12");
            }

            double factor = 1.0 / (dpt11 * dpt22 - dpt12 * dpt12);

            /*
            -- Compute the two gradients.
            g1 = (dv1 * dpt22 - dv2 * dpt12) * factor
            g2 = (dv2 * dpt11 - dv1 * dpt12) * factor
            */
            Vector g1 = Vector.VectorMul(factor, Vector.VectorDiff(Vector.VectorMul(dpt22, dv1), Vector.VectorMul(dpt12, dv2)));
            Vector g2 = Vector.VectorMul(factor, Vector.VectorDiff(Vector.VectorMul(dpt11, dv2), Vector.VectorMul(dpt12, dv1)));
            //ASE::GeomPoint g1 = ((dv1 * dpt22) - (dv2 * dpt12)) * factor;
            //ASE::GeomPoint g2 = ((dv2 * dpt11) - (dv1 * dpt12)) * factor;

            /*
            p_grad_u = dpt1 * g1.x + dpt2 * g2.x
            p_grad_v = dpt1 * g1.y + dpt2 * g2.y
            */
            Vector p_grad_u = Vector.VectorAdd(Vector.VectorMul(g1.X, dpt1), Vector.VectorMul(g2.X, dpt2));
            Vector p_grad_v = Vector.VectorAdd(Vector.VectorMul(g1.Y, dpt1), Vector.VectorMul(g2.Y, dpt2));
            //ASE::GeomPoint p_grad_u = dpt1 * g1.x + dpt2 * g2.x;
            //ASE::GeomPoint p_grad_v = dpt1 * g1.y + dpt2 * g2.y;

            /*
            -- Repeat process above, computing just one vector in the plane.
            dup1 = dot dpt1 p_grad_u
            dup2 = dot dpt2 p_grad_u
            dvp1 = dot dpt1 p_grad_v
            dvp2 = dot dpt2 p_grad_v
            */
            double dup1 = Vector.VectorDot(dpt1, p_grad_u);
            double dup2 = Vector.VectorDot(dpt2, p_grad_u);
            double dvp1 = Vector.VectorDot(dpt1, p_grad_v);
            double dvp2 = Vector.VectorDot(dpt2, p_grad_v);

            /*
            fuctor = 1.0 / (dup1 * dvp2 - dvp1 * dup2)
            */

            /*  Impossible values may occur here, and cause divide by zero problems.
                Handle these by setting the divisor to a safe value then flagging
                thatr it is impossible. Impossible textured polygons use the normal 
                to the polygon, which makes no texture appear.
                26-12-01
            */
            double divisor = (dup1 * dvp2 - dvp1 * dup2);
            bool impossible1 = Math.Abs(divisor) <= MINIMUM_DIVISOR;

            if (impossible1)
            {
                divisor = 1.0;
            }

            double fuctor = 1.0 / divisor;

            /*
            b1 = (val0.x * dvp2 - val0.y * dup2) * fuctor
            b2 = (val0.y * dup1 - val0.x * dvp1) * fuctor
            */
            double b1 = (ds0 * dvp2 - dt0 * dup2) * fuctor;
            double b2 = (dt0 * dup1 - ds0 * dvp1) * fuctor;

            /*
            p_base = pt0 - (dpt1 * b1 + dpt2 * b2)
            */
            Vector p_base = Vector.VectorDiff(pt0, Vector.VectorAdd(Vector.VectorMul(b1, dpt1), Vector.VectorMul(b2, dpt2)));

            /*
            p_grad_u *= 256
            p_grad_v *= 256
            */
            p_grad_u = Vector.VectorMul(textureWidth, p_grad_u);
            p_grad_v = Vector.VectorMul(textureHeight, p_grad_v);

            //
            // Calculate Normals. These are ignored anyway but make an effort...
            //
            Vector pA = Vector.VectorDiff(pt1, pt0);
            Vector pB = Vector.VectorDiff(pt2, pt0);

            Vector pN = Vector.VectorCross(pA, pB);
            pN = Vector.VectorNormalize(pN);

            double dnx = -pN.X;
            double dny = pN.Y;
            double dnz = pN.Z;

            //
            // Check for error values
            //
            bool impossible2 = double.IsNaN(p_base.X) || double.IsNaN(p_base.Y) || double.IsNaN(p_base.Z) ||
                               double.IsNaN(p_grad_v.X) || double.IsNaN(p_grad_v.Y) || double.IsNaN(p_grad_v.Z) ||
                               double.IsNaN(p_grad_v.X) || double.IsNaN(p_grad_v.Y) || double.IsNaN(p_grad_v.Z);

            bool impossible = impossible1 || impossible2;

            if (impossible)
            {
                origin[0] = pt1.X;
                origin[1] = -pt1.Y;
                origin[2] = pt1.Z;
            }
            else
            {
                origin[0] = p_base.X;
                origin[1] = -p_base.Y;
                origin[2] = p_base.Z;
            }

            if (!impossible)
            {
                textureU[0] = p_grad_u.X;
                textureU[1] = -p_grad_u.Y;
                textureU[2] = p_grad_u.Z;

                textureV[0] = p_grad_v.X;
                textureV[1] = -p_grad_v.Y;
                textureV[2] = p_grad_v.Z;
            }
            else
            {
                textureU[0] = dnx;
                textureU[1] = dny;
                textureU[2] = dnz;

                textureV[0] = dnx;
                textureV[1] = dny;
                textureV[2] = dnz;
            }

            return (textureU, textureV, origin);
        }
    }
}
