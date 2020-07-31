using System;
using System.Collections.Generic;
using Mautom.Portunus.Entities.Models;

namespace Mautom.Portunus.Contracts
{
    public interface IAddressVerificationRepository
    {
        public IEnumerable<AddressVerification> GetAllAddressVerifications(bool trackChanges = true);
        public IEnumerable<AddressVerification> GetAddressVerificationsByToken(Guid token, bool trackChanges = true);
        public AddressVerification GetAddressVerificationByEmail(string email, bool trackChanges = true);

        public void Create(AddressVerification addressVerification);
        public void Remove(AddressVerification addressVerification);
    }
}