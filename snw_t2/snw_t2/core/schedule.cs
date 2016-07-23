using System; 
namespace snw.core
{ 
    public class schedule
    {
        public schedule()
        {
            CreationTime = DateTime.Now;
        }
        public DateTime CreationTime { set; get; }
        public int ExpirationMinutes { set; get; } 
    }
}