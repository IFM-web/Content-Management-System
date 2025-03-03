using System.Collections.Generic;

namespace ContentManagementSystem.Models
{
    public class MaterialViewModel
    {
        public MaterialViewModel()
        {
            Materials = new List<Material>();
            NewMaterial = new Material();
        }

        public List<Material> Materials { get; set; }
        public Material NewMaterial { get; set; }
    }
}
