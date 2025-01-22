namespace SneakersPlanet.Models
{
    public class CartItem
    {
        public string SneakerId { get; set; }
        public string Name { get; set; }
        public string Size { get; set; }
        public decimal Price { get; set; }
        public string ImageURL { get; set; }
        public int RemainingStock { get; set; }
    }
    public class Cart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();
    }
}