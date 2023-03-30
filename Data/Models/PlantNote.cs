using System;
using System.ComponentModel.DataAnnotations;

namespace Plants.info.API.Data.Models
{
	public class PlantNote
	{
		public int Id { get; set; }
        public int UserId { get; set; }
        public int PlantId { get; set; }
        [Required]
        [MaxLength(200)]
        public string? Description { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateEdited { get; set; }
    }
}

