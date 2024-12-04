﻿using System;
using System.ComponentModel.DataAnnotations;

namespace UF5423_Aguas.Data.API
{
    public class ConsumptionDto
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        [Required]
        public int Volume { get; set; }

        public string Status { get; set; }
    }
}
