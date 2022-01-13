using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent an (infinite) plane in a scene.
    /// </summary>
    public class Sphere : SceneEntity
    {
        private Vector3 center;
        private double radius;
        private Material material;

        /// <summary>
        /// Construct a sphere given its center point and a radius.
        /// </summary>
        /// <param name="center">Center of the sphere</param>
        /// <param name="radius">Radius of the spher</param>
        /// <param name="material">Material assigned to the sphere</param>
        public Sphere(Vector3 center, double radius, Material material)
        {
            this.center = center;
            this.radius = radius;
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the sphere, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        {
            // Write your code here...

            // Get discriminant of quadratic with b^2 -4 * a * c;
            Vector3 dist = ray.Origin - this.center;
            double a = ray.Direction.Normalized().Dot(ray.Direction.Normalized());
            double b = 2 * ray.Direction.Normalized().Dot(dist);
            double c = dist.Dot(dist) - radius * radius;
            
            double discriminant = b * b - 4 * a * c;
            if(discriminant < 0){
                return null;
            }
            else {
                Vector3 incident = ray.Direction.Normalized();

                double numerator = -b - Math.Sqrt(discriminant);
                if(numerator > 0){
                    Vector3 intersection = ray.Origin + ray.Direction.Normalized() * (numerator / (2 * a));
                    Vector3 normal  = (intersection - this.center).Normalized();
                    
                    RayHit output = new RayHit(intersection,normal,incident,this.material);
                    return output;

                }
                
                // Make sure it is the closest point in front of the ray
                numerator = -b + Math.Sqrt(discriminant);
                if(numerator > 0){
                    Vector3 intersection = ray.Origin + ray.Direction.Normalized() * (numerator / 2 * a);
                    Vector3 normal  = (intersection - this.center).Normalized();
                    
                    RayHit output = new RayHit(intersection,normal,incident,this.material);
                    return output;
                }
                
                return null;
                
            }

        }

        /// <summary>
        /// The material of the sphere.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
