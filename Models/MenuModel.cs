namespace Menu.Models
{
    public class MenuModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Detail { get; set; }
        public int Price { get; set; }
        public IFormFile Image { get; set; }    
        public string? ImgUrl { get; set; }
        public int CategoryId { get; set; } 
        public string? CategoryName { get; set; }       
    }
}
