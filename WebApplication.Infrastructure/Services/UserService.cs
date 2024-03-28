using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApplication.Infrastructure.Contexts;
using WebApplication.Infrastructure.Entities;
using WebApplication.Infrastructure.Interfaces;

namespace WebApplication.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly InMemoryContext _dbContext;

        public UserService(InMemoryContext dbContext)
        {
            _dbContext = dbContext;

            // this is a hack to seed data into the in memory database. Do not use this in production.
            _dbContext.Database.EnsureCreated();
        }

        /// <inheritdoc />
        public async Task<User?> GetAsync(int id, CancellationToken cancellationToken = default)
        {
            User? user = await _dbContext.Users.Where(user => user.Id == id)
                                         .Include(x => x.ContactDetail)
                                         .FirstOrDefaultAsync(cancellationToken);

            return user;
        }

        /// <inheritdoc />
        /// Implement a way to find users that match the provided given names OR last name.
        /// 
        public async Task<IEnumerable<User>> FindAsync(string? givenNames, string? lastName, CancellationToken cancellationToken = default)
        {
            return await _dbContext
                .Users
                .Where(p => p.GivenNames.Equals(givenNames) || p.LastName.Equals(lastName))
                .Include(p=>p.ContactDetail)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        /// Implement a way to get a 'page' of users.
        public async Task<IEnumerable<User>> GetPaginatedAsync(int page, int count, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .Skip((page - 1) * count)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        /// Implement a way to add a new user, including their contact details.
        public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
        {
            await _dbContext.Users.AddAsync(user, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return user;
        }

        /// <inheritdoc />
        /// Implement a way to update an existing user, including their contact details.
        public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            //var entity = _dbContext.Users.Where(p=> p.Id.Equals(user.Id)).Include(p=>p.ContactDetail).FirstOrDefaultAsync(cancellationToken);
            var entity = await _dbContext.Users.Include(p=>p.ContactDetail).FirstOrDefaultAsync(p => p.Id.Equals(user.Id), cancellationToken);
            if(entity != null)
            {
                entity.GivenNames = user.GivenNames;
                entity.LastName = user.LastName;

                if(user.ContactDetail != null)
                {
                    if(entity.ContactDetail == null)
                        entity.ContactDetail = new ContactDetail();

                    entity.ContactDetail.UserId = user.Id;
                    entity.ContactDetail.MobileNumber = user.ContactDetail.MobileNumber;
                    entity.ContactDetail.EmailAddress = user.ContactDetail.EmailAddress;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return entity;
        }

        /// <inheritdoc />
        /// Implement a way to delete an existing user, including their contact details.
        public async Task<User?> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Users.FirstOrDefaultAsync(p => p.Id.Equals(id), cancellationToken);
            if(entity != null)
            {
                _dbContext.Users.Remove(entity);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return entity;
        }

        /// <inheritdoc />
        /// Implement a way to count the number of users in the database.
        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users.Select(p=>p.Id).CountAsync(cancellationToken);
        }

        public async Task<bool> CheckUserExists(int id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users.AnyAsync(p => p.Id.Equals(id));
        }
    }
}
