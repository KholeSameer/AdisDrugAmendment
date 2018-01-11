

namespace DrugAmmendment.Services
{
   
        public interface IAuthorizationProvider
        {
            bool IsInRole(string role);
            bool IsAdmin { get; }
            bool IsUser { get; }
            bool IsEditor { get; }
            string Username { get; }
        }
    
}
