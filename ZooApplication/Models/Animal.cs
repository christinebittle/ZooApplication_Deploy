using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZooApplication.Models
{
    public class Animal
    {
        [Key]
        public int AnimalID { get; set; }

        [Required]
        public string AnimalName { get; set; }

        //weight is in kg
        [Required]
        public int AnimalWeight { get; set; }
        
        //data needed for keeping track of animal images uploaded
        //images deposited into /Content/Images/Animals/{id}.{extension}
        public bool AnimalHasPic { get; set; }
        public string PicExtension { get; set; }

        [AllowHtml]
        public string AnimalBio { get; set; }

        public Sex AnimalSex { get; set;}


        //An animal belongs to one species
        //A species can have many animals
        [ForeignKey("Species")]
        public int SpeciesID { get; set; }
        public virtual Species Species { get; set; }


        //an animal can be taken care of by many keepers
        public ICollection<Keeper> Keepers { get; set; }

    }

    public class AnimalDto
    {
        public int AnimalID { get; set; }
        
        [Required(ErrorMessage = "Please Enter a Name.")]
        public string AnimalName { get; set; }

        //weight is in kg
        [Required(ErrorMessage = "Please Enter a Weight.")]
        public int AnimalWeight { get; set; }

        [AllowHtml]
        public string AnimalBio { get; set; }

        public int SpeciesID { get; set; }
        public string SpeciesName { get; set; }

        //data needed for keeping track of animals images uploaded
        //images deposited into /Content/Images/Animals/{id}.{extension}
        public bool AnimalHasPic { get; set; }
        public string PicExtension { get; set; }


    }

    public enum Sex
    {
        Male,
        Female
    }

}