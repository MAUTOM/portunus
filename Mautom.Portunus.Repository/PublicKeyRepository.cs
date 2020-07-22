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

using System;
using System.Collections.Generic;
using System.Linq;
using Mautom.Portunus.Contracts;
using Mautom.Portunus.Entities;
using Mautom.Portunus.Entities.Models;
using Mautom.Portunus.Shared.Pgp;
using Microsoft.EntityFrameworkCore;

namespace Mautom.Portunus.Repository
{
    public class PublicKeyRepository : RepositoryBase<PublicKey>, IPublicKeyRepository
    {
        public PublicKeyRepository(RepositoryContext context) : base(context)
        {
            
        }

        public IEnumerable<PublicKey> GetAllPublicKeys(bool trackChanges = true)
        {
            return FindAll(trackChanges)
                .OrderBy(pk => pk.SubmissionDate)
                .Include(pk => pk.KeyIdentities)
                .ToList();
        }

        public PublicKey? GetPublicKeyByFingerprint(string fingerprint, bool trackChanges = true)
        {
            return FindByCondition(key => key.Fingerprint.Equals(new PublicKeyFingerprint(fingerprint)), trackChanges)
                .Include(pk => pk.KeyIdentities)
                .SingleOrDefault();
        }

        public PublicKey? GetPublicKeyByKeyId(string keyId, bool trackChanges = true)
        {
            if (keyId.Length == 40)
                return FindByCondition(key => key.Fingerprint.Equals(new PublicKeyFingerprint(keyId)), trackChanges)
                    .Include(pk => pk.KeyIdentities)
                    .SingleOrDefault();
            
            return FindByCondition(
                    key => key.LongKeyId.Equals(keyId, StringComparison.OrdinalIgnoreCase) || key.ShortKeyId.Equals(keyId, StringComparison.OrdinalIgnoreCase),
                    trackChanges)
                .Include(pk => pk.KeyIdentities)
                .FirstOrDefault();
        }
        
        public void CreatePublicKey(PublicKey key)
        {
            Create(key);
        }
    }
}