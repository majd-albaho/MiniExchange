namespace AuthService.Domain.Entities
{
    public class EntityBase
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTimeOffset CreatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;

        public DateTimeOffset ModifiedDate { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;

        public DateTimeOffset DeletedDate { get; set; }
        public string DeletedBy { get; set; } = string.Empty;
    }
}
