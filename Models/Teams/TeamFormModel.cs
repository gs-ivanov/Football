namespace Tournament.Models.Teams
{
    using System.ComponentModel.DataAnnotations;

    using static Data.DataConstants.Team;

    public class TeamFormModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(NameMaxLength, MinimumLength = NameMinLength)]
        public string Name { get; init; }

        [Required]
        [StringLength(CityMaxLength, MinimumLength = CityMinLength)]
        public string City { get; init; }

        [Required]
        public string Trener { get; init; }

        //[Required]
        //[Display(Name = "Winning points:")]
        //public int Wins { get; init; } = 0;

        //[Required]
        //[Display(Name = "Lost points:")]
        //public int Losts { get; init; } = 0;

    }
}