using System;
using System.ComponentModel.DataAnnotations;

namespace Plants.info.API.Data.Models
{
	public class PlantEdit
	{
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        public int GenusId { get; set; }
        public DateTime DateWatered { get; set; }
        public DateTime DateFertilized { get; set; }
        public int WaterInterval { get; set; }
        public int FertilizeInterval { get; set; }
        public PlantImageEdit? Image { get; set; }
    }
}

