using System;
using Plants.info.API.Models;

namespace Plants.info.API.Data.Models
{
    // Utilize this class to present plant information including plant statistics 
	public class PlantInfo : Plant 
	{
        public string GenusName { get; set; }
        public PlantImage? Image { get; set; }
    }
}

