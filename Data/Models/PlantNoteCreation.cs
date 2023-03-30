using System;
using System.ComponentModel.DataAnnotations;

namespace Plants.info.API.Data.Models
{
	public class PlantNoteCreation
	{
        [Required]
        [MaxLength(200)]
        public string? Description { get; set; }
    }
}

