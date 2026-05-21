namespace SharedLibrary.Entities
{
    public class EntityBase<T>
    {
        public required T Id { get; set; }

        public required DateTimeOffset CreatedDate { get; set; }
        public required string CreatedBy { get; set; }

        public DateTimeOffset ModifiedDate { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;

        public DateTimeOffset DeletedDate { get; set; }
        public string DeletedBy { get; set; } = string.Empty;
    }
}
