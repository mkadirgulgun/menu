namespace Menu.Models
{
    public class Login
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }    
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public int? IsApproved { get; set; }    
        public string? ValidKey { get; set; }       
        public string? ResetKey { get; set; }
    }
    
}
