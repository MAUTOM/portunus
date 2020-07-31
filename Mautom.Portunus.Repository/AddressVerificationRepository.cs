using System;
using System.Collections.Generic;
using System.Linq;
using Mautom.Portunus.Contracts;
using Mautom.Portunus.Entities;
using Mautom.Portunus.Entities.Models;

namespace Mautom.Portunus.Repository
{
    public class AddressVerificationRepository : RepositoryBase<AddressVerification>, IAddressVerificationRepository
    {
        public AddressVerificationRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }

        public IEnumerable<AddressVerification> GetAllAddressVerifications(bool trackChanges = true)
        {
            return FindAll(trackChanges).OrderBy(av => av.VerificationId).ToList();
        }

        public IEnumerable<AddressVerification> GetAddressVerificationsByToken(Guid token, bool trackChanges = true)
        {
            return FindByCondition(av => av.Token.Equals(token), trackChanges).OrderBy(av => av.VerificationId);
        }

        public AddressVerification GetAddressVerificationByEmail(string email, bool trackChanges = true)
        {
            return FindByCondition(av => av.Email.Equals(email, StringComparison.OrdinalIgnoreCase), trackChanges)
                .FirstOrDefault();
        }

        public void Remove(AddressVerification addressVerification)
        {
            Delete(addressVerification);
        }
    }
}