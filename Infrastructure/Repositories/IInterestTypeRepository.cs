using ManageAccount.Data.Entities;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Interface cho InterestType Repository
    /// </summary>
    public interface IInterestTypeRepository
    {
        InterestType? GetById(int id);
        InterestType? GetByRate(decimal rate);
        InterestType Add(InterestType interestType);
        void Delete(InterestType interestType);
        int SaveChanges();
    }
}