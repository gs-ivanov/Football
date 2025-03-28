namespace Tournament.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    using static DataConstants.Team;

    public class Team
    {
        public int Id { get; set; }

        [Required]
        [StringLength(NameMaxLength, MinimumLength = NameMinLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(CityMaxLength, MinimumLength = CityMinLength)]
        public string City { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Trener")]
        public string Trener { get; set; }

        public int Wins { get; set; }
        public int Losts { get; set; }
        public int Draws { get; set; }  // Ново поле за равни мачове

        public int GoalsScored { get; set; }
        public int GoalsConceded { get; set; }
        public int Points { get; set; }

        public int GoalDifference => GoalsScored - GoalsConceded;

    }
}
