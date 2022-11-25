using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plants.info.API.Data.Models
{
    public class PlantCreation
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        public string Genus { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime DateWatered { get; set; }
        public DateTime DateFertilized { get; set; }
        public int WaterInterval { get; set; }
        public int FertilizeInterval { get; set; }
    }
}
