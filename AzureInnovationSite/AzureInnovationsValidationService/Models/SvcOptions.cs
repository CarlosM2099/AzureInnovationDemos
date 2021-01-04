using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AzureInnovationsValidationService.Models
{
    
    public class SvcOptions
    {
        [Required]
        public string PowerAppsCognitiveAzFuncURL { get; set; }
        [Required]
        public string PowerAppsSparesAzFuncURL { get; set; }
    }
}