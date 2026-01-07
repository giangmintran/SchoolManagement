namespace SchoolManagement.Data.Entities
{
    public class PrivacyPolicy
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate = DateTime.Now;
        public DateTime? ModifiedDate {  get; set; }
    }
}
