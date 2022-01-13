using System;
using System.Collections.Generic;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a ray traced scene, including the objects,
    /// light sources, and associated rendering logic.
    /// </summary>
    public class Scene
    {
        private SceneOptions options;
        private ISet<SceneEntity> entities;
        private ISet<PointLight> lights;

        /// <summary>
        /// Construct a new scene with provided options.
        /// </summary>
        /// <param name="options">Options data</param>
        public Scene(SceneOptions options = new SceneOptions())
        {
            this.options = options;
            this.entities = new HashSet<SceneEntity>();
            this.lights = new HashSet<PointLight>();
        }

        /// <summary>
        /// Add an entity to the scene that should be rendered.
        /// </summary>
        /// <param name="entity">Entity object</param>
        public void AddEntity(SceneEntity entity)
        {
            this.entities.Add(entity);
        }

        /// <summary>
        /// Add a point light to the scene that should be computed.
        /// </summary>
        /// <param name="light">Light structure</param>
        public void AddPointLight(PointLight light)
        {
            this.lights.Add(light);
        }

        /// <summary>
        /// Render the scene to an output image. This is where the bulk
        /// of your ray tracing logic should go... though you may wish to
        /// break it down into multiple functions as it gets more complex!
        /// </summary>
        /// <param name="outputImage">Image to store render output</param>
        public void Render(Image outputImage)
        {
            // Begin writing your code here...
            
            Random rng = new Random();

            // The origin point (camera)
            Vector3 origin = new Vector3(0,0,0);
            
            // Field of view in radians
            double fov = (Math.PI * 60)/180.0;

            // Aspect ratio
            double aspect_ratio = outputImage.Width/outputImage.Height;

            Color white = new Color(1,1,1);

            // variables for the AA multiplyer 
            double multi = options.AAMultiplier;
            double iter = multi * multi;
            double inc = ((1/multi)/2);

            // Depth of View
            double focalLen = options.FocalLength;
            double ar = options.ApertureRadius;

            // Fire a ray for each pixel
            for (int y = 0; y < outputImage.Height; y++){
                for (int x = 0; x < outputImage.Width; x++){
                    
                    Color colorAA = new Color(0,0,0);

                    // Anti-aliasing
                    double y_count = inc;
                    for (int iy = 0;iy<multi;iy++){
                        double x_count = inc;

                        for  (int ix = 0;ix<multi;ix++){
                            double pix_loc_x = (x+x_count) / outputImage.Width;
                            double pix_loc_y = (y+y_count) / outputImage.Height;

                            x_count+=(2*inc);

                            double x_pos = (pix_loc_x * 2)-1;
                            double y_pos = 1-(pix_loc_y * 2);

                            x_pos = x_pos * Math.Tan(fov/2);
                            y_pos = y_pos * (Math.Tan(fov/2) / aspect_ratio);

                            Vector3 ray_direction = new Vector3(x_pos,y_pos,1);
                            Ray ray = new Ray(origin,ray_direction);

                            if(ar!=0){
                            // Focal point
                                Vector3 focalPoint = ray_direction.Normalized() * focalLen;
                                
                                // Out of focus
                                double blur = (rng.NextDouble()+(-0.5))* 2 * ar;
                                Vector3 newOrig = new Vector3(blur,blur,0);
                                Vector3 blurDirec = focalPoint - newOrig;
                                Ray ray0 = new Ray(newOrig,blurDirec);
                                
                                double maxDepth = 10;
                                Color outColour = RRTrace(ray0, maxDepth);
                                colorAA += outColour;
                            }
                            else{
                                double maxDepth = 10;
                                Color outColour = RRTrace(ray, maxDepth);
                                colorAA += outColour;
                            }
                        }
                        y_count+= (2*inc);
                    }
                    colorAA = colorAA/iter;
                    outputImage.SetPixel(x,y,colorAA);

                }
            }
        }

        /****** Hnadle Reflective and Refractive Matrials recursively ******/
        Color RRTrace(Ray ray, double depth){
            
            Color outColour = new Color(0,0,0);
            
            if (depth > 0){
                
                // Check for closest object to the ray
                double closest = -1;
                bool colorFound = false;
                bool fesnel =false;
                Ray newRay = ray;
                Ray newRay1 = ray;
                double reflectP=0;
                double refractP=0;

                
                foreach(SceneEntity entity in this.entities){
                    RayHit hit = entity.Intersect(ray);
                    if(hit != null){
                        
                        // Check for closest object
                        if(closest== -1 || (hit.Position-ray.Origin).Length() < closest){
                            closest = (hit.Position-ray.Origin).Length();
                            
                            // Base case
                            // Hit object which is diffusive
                            if (entity.Material.Type.Equals(Material.MaterialType.Diffuse)){
                                
                                // Check for shadow
                                Color mColour = new Color(0,0,0);

                                foreach (PointLight light in this.lights){
                                    Vector3 N = entity.Intersect(ray).Normal;
                                    Vector3 L = (light.Position - hit.Position).Normalized();
                                    

                                    // Check for shadow
                                    bool blocked = false;
                                    Vector3 offsetHit = hit.Position+(0.0001*hit.Normal);
                                    Ray shadowCheck = new Ray(offsetHit,L);
                                    

                                    foreach (SceneEntity e in this.entities){
                                        RayHit shadowHit = e.Intersect(shadowCheck);
                                        
                                        // Something is blocking the light
                                        if(shadowHit != null ){
                                            double between = (shadowHit.Position-offsetHit).Dot(light.Position-shadowHit.Position);
                                            if(between>0){
                                                blocked = true;
                                            }
                                        }
                                    }

                                    // Light on material
                                    Color lightDense = entity.Material.Color * light.Color * N.Dot(L);
                                    if (blocked == false ){  
                                        mColour += lightDense;
                                    }
                                }

                                // Check and remove colour overflow
                                            double R = mColour.R;
                                            double G = mColour.G;
                                            double B = mColour.B;

                                            if(mColour.R < 0){
                                                R = 0;
                                            }
                                            if(mColour.G < 0){
                                                G = 0;
                                            }
                                            if(mColour.B < 0){
                                                B = 0;
                                            }

                                            if(mColour.R > 1){
                                                R = 1;
                                            }
                                            if(mColour.G > 1){
                                                G = 1;
                                            }
                                            if(mColour.B > 1){
                                                B = 1;
                                            }

                                            outColour = new Color(R,G,B);
                                            colorFound = true;
                                                                 
                            }

                            // Hits material that is refractive
                            if (entity.Material.Type.Equals(Material.MaterialType.Refractive)){
                            
                                double idx = entity.Material.RefractiveIndex;
                                double cosI = hit.Incident.Normalized().Dot(hit.Normal);
                                Vector3 out_normal = hit.Normal;

                                // Going from inside to outside
                                if(hit.Incident.Dot(hit.Normal)>0){
                                    out_normal = hit.Normal;
                                    idx = entity.Material.RefractiveIndex;
                                    cosI = hit.Incident.Normalized().Dot(hit.Normal);
                                    
                                }

                                // Going from outside to inside
                                else{
                                    out_normal = -hit.Normal;
                                    idx = 1.0/entity.Material.RefractiveIndex;
                                    cosI = -(hit.Incident.Normalized().Dot(hit.Normal));

                                }
                                double sinT2 = idx*idx*(1.0-cosI*cosI);
                                double cosT = Math.Sqrt(1.0-sinT2);

                                Vector3 refract = hit.Incident*idx - out_normal * (-cosT+idx*cosI);
                                Vector3 offsetHit = hit.Position+(0.0001*refract);   
                                newRay = new Ray(offsetHit,refract);
                                colorFound = false;

                                // The Fresnel effect
                                fesnel=true; // Turn the effect on

                                Vector3 uv = hit.Incident.Normalized();
                                double dt = uv.Dot(hit.Normal);
                                double dis = (1.0 - idx*idx*(1-dt*dt));
                                if(dis>0){
                                    reflectP=Schlick(cosI,idx);
                                    refractP = 1-reflectP;
                                }
                                // Total reflection
                                else{
                                    reflectP  = 1.0; 
                                }
                                
                            }

                            // Hits material that is reflective
                            if(entity.Material.Type.Equals(Material.MaterialType.Reflective) || reflectP > 0){
                                
                                Vector3 reflect = hit.Incident - 2 * hit.Incident.Dot(hit.Normal) * hit.Normal;
                                Vector3 offsetHit = hit.Position+(0.0001*hit.Normal);
                                if(entity.Material.Type.Equals(Material.MaterialType.Reflective)){
                                newRay = new Ray(offsetHit,reflect);
                                }
                                newRay1 = new Ray(offsetHit,reflect);
                                colorFound = false;
                            }

                            // Hit material that is Glossy
                            if(entity.Material.Type.Equals(Material.MaterialType.Glossy)){

                                // Reflect component
                                Vector3 reflect = hit.Incident - 2 * hit.Incident.Dot(hit.Normal) * hit.Normal;
                                Vector3 offsetHit = hit.Position+(0.0001*hit.Normal);
                                Ray newR = new Ray(offsetHit,reflect);
                                Color ref1 = RRTrace(newR,depth-1);

                                // Self Colour component
                                Color mColour = new Color(0,0,0);

                                foreach (PointLight light in this.lights){
                                    Vector3 N = entity.Intersect(ray).Normal;
                                    Vector3 L = (light.Position - hit.Position).Normalized();
                                    

                                    // Check for shadow
                                    bool blocked = false;
                                    offsetHit = hit.Position+(0.0001*hit.Normal);
                                    Ray shadowCheck = new Ray(offsetHit,L);
                                    

                                    foreach (SceneEntity e in this.entities){
                                        RayHit shadowHit = e.Intersect(shadowCheck);
                                        
                                        // Something is blocking the light
                                        if(shadowHit != null ){
                                            double between = (shadowHit.Position-offsetHit).Dot(light.Position-shadowHit.Position);
                                            if(between>0){
                                                blocked = true;
                                            }
                                        }
                                    }

                                    // Light on material
                                    Color lightDense = entity.Material.Color * light.Color * N.Dot(L);
                                    if (blocked == false ){
                                        mColour += lightDense;
                                    }
                                }
                                mColour=mColour+(ref1/4.0);

                                // Check and remove colour overflow
                                            double R = mColour.R;
                                            double G = mColour.G;
                                            double B = mColour.B;

                                            if(ref1.R < 0){
                                                R = 0;
                                            }
                                            if(mColour.G < 0){
                                                G = 0;
                                            }
                                            if(mColour.B < 0){
                                                B = 0;
                                            }

                                            if(mColour.R > 1){
                                                R = 1;
                                            }
                                            if(mColour.G > 1){
                                                G = 1;
                                            }
                                            if(mColour.B > 1){
                                                B = 1;
                                            }

                                            outColour = new Color(R,G,B);
                                            colorFound = true;

                            }
                            
                        }
                    }
                }

                // Hit a solid color
                if(colorFound == true){
                    return outColour;
                }
                
                // Hit a refractive material
                else if(fesnel==true){
                    // Total reflection
                    if(reflectP == 1){
                        
                        return RRTrace(newRay1,depth-1);
                    }
                    // Both reflection and refraction
                    else{ 
                        Color color1 = RRTrace(newRay1,depth-1);
                        Color color2 = RRTrace(newRay,depth-1);
                        Color output = new Color(reflectP * color1.R+refractP*color2.R,reflectP * color1.G+refractP*color2.G,reflectP * color1.B+refractP*color2.B);
                        return output;
                    }
                    
                }
                // Hit a relfective material  without fesnel
                else {
                    return RRTrace(newRay,depth-1);
                }
            }
            return outColour;
        }

        // Schlick's approximation to estimate contribution of reflection in Fresnel factor
        double Schlick(double cos, double idx){
            double r0 = (1-idx)/(1+idx);
            r0 = r0*r0;
            return r0+(1-r0)*Math.Pow((1-cos),5);
        }              
    }
}


