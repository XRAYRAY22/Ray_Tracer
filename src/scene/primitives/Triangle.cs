using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a triangle in a scene represented by three vertices.
    /// </summary>
    public class Triangle : SceneEntity
    {
        private Vector3 v0, v1, v2;
        private Material material;

        /// <summary>
        /// Construct a triangle object given three vertices.
        /// </summary>
        /// <param name="v0">First vertex position</param>
        /// <param name="v1">Second vertex position</param>
        /// <param name="v2">Third vertex position</param>
        /// <param name="material">Material assigned to the triangle</param>
        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Material material)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the triangle, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        {
        //     // Write your code here...
        
            Vector3 v1v0 = (this.v1-this.v0);
            Vector3 v2v0 = (this.v2-this.v0);

            // Normal of the triangle
            Vector3 normal = v1v0.Cross(v2v0);
            
            Vector3 p = (ray.Direction.Normalized()).Cross(v2v0);
            double determinant = v1v0.Dot(p);

            //Check if there is a intersection
            if (determinant < 0.0001 && determinant > -0.0001){
                return null;
            }

            // Moller Trumbore intersection algorithm
            Vector3 tvec = ray.Origin - v0;
            double u = tvec.Dot(p) * (1.0/determinant);

            if (u < 0.0 ||u > 1.0){
                return null;
            }

            Vector3 qvec = tvec.Cross(v1v0);
            double v = (ray.Direction.Normalized()).Dot(qvec) * (1.0/determinant);
            
            if (v < 0.0 || u+v > 1.0){
                return null;
            }

            
            double t = v2v0.Dot(qvec) * (1.0/determinant);
            //double t = (v0-ray.Origin).Dot(normal)/(ray.Direction.Dot(normal));
            
            if(t>0.0001){
                Vector3 intersection = ray.Origin + t * ray.Direction.Normalized();

                // The ray does hit the triangle
                Vector3 incident = ray.Direction.Normalized();
                RayHit output = new RayHit(intersection,normal.Normalized(),incident,this.material);
                return output;
            }
            return null;       
        }

        /// <summary>
        /// The material of the triangle.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
