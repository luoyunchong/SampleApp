namespace SampleApi.Models
{
    public class RefreshToken
    {
        public int UserId { get; set; }
        public string Token { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime ExpiredTime { get; set; }
        public bool IsExpired
        {
            get
            {
                return DateTime.Now > ExpiredTime;
            }
        }
    }
}
