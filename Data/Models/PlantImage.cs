using System;
namespace Plants.info.API.Data.Models
{
	public class PlantImage
	{
        public int Id { get; set; }
        public string Name { get; set; }
        public int UserId { get; set; }
        public int PlantId { get; set; }
        public string Type { get; set; }
        public Int64 Size { get; set; }
        public string Base64 { get; set; }
    }
}

