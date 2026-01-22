namespace MembershipOperations.Domain.Entities
{
    public class MembershipPlan
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal MonthlyPrice { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
