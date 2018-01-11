
namespace DrugAmmendment.Services
{
    public interface ISecureController
    {
        IAuthorizationProvider AuthorisationProvider { get; }
    }
}
