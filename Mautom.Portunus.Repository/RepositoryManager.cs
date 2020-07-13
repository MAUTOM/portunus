// Copyright (c) 2020 Bence Horváth <horvath.bence@mautom.hu>.
//
// This file is part of Portunus OpenPGP key server 
// (see https://www.horvathb.dev/portunus).
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
using Mautom.Portunus.Contracts;
using Mautom.Portunus.Entities;

namespace Mautom.Portunus.Repository
{
    public class RepositoryManager : IRepositoryManager
    {
        private readonly RepositoryContext _repoContext;
        private IPublicKeyRepository _publicKeyRepository = null!;
        private IKeyIdentityRepository _keyIdentityRepository = null!;
        
        public RepositoryManager(RepositoryContext context)
        {
            _repoContext = context;
        }

        public IPublicKeyRepository PublicKey => _publicKeyRepository ??= new PublicKeyRepository(_repoContext);
        public IKeyIdentityRepository KeyIdentity => _keyIdentityRepository ??= new KeyIdentityRepository(_repoContext);

        public void Save() => _repoContext.SaveChanges();
    }
}