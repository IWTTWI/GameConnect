namespace GameConnect.Models
{
    public class User{
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public decimal Balance { get; set; }
        public int MidnightCoins { get; set; }
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        public List<UserItem> Items { get; set; } = new();}
    public class UserItem{
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;}}