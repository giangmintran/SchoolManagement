namespace SchoolManagement.Data
{
    public interface IAuditable
    {
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
