using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plants.info.API.Models
{
    public class Plant
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Genus { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime DateWatered { get; set; }
        public DateTime DateFertilized { get; set; }
        public int WaterInterval { get; set; }
        public int FertilizeInterval { get; set; }
        public int UserId { get; set; }
    }
}
