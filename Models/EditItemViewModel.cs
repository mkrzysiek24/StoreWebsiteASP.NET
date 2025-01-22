using System;
using System.Collections.Generic;

namespace SneakersPlanet.Models
{
    public class EditItemViewModel
    {
        public string Id { get; set; }
        public string CategoryName { get; set; }
        public string Brand { get; set; }
        public string Gender { get; set; }
        public string ImageURL { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Slug { get; set; }
        
        public List<Size> Sizes { get; set; }
    }


    public class Size
    {
        public string SizeValue { get; set; }
        public int Quantity { get; set; }
    }
}
