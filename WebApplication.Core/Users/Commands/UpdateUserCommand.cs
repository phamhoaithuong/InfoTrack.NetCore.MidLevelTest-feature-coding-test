using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using WebApplication.Core.Users.Common.Models;
using WebApplication.Infrastructure.Entities;
using WebApplication.Infrastructure.Interfaces;

namespace WebApplication.Core.Users.Commands
{
    public class UpdateUserCommand : IRequest<UserDto>
    {
        public int Id { get; set; }
        public string GivenNames { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;

        public class Validator : AbstractValidator<UpdateUserCommand>
        {
            private readonly IUserService _userService;
            public Validator(IUserService userService)
            {
                // TODO: Create validation rules for UpdateUserCommand so that all properties are required.
                // If you are feeling ambitious, also create a validation rule that ensures the user exists in the database.
                _userService = userService;
                RuleFor(x => x.GivenNames)
                        .NotEmpty();

                RuleFor(x => x.LastName)
                    .NotEmpty();

                RuleFor(x => x.EmailAddress)
                    .NotEmpty();

                RuleFor(x => x.MobileNumber)
                    .NotEmpty();

                RuleFor(t => t.Id).NotEmpty().MustAsync(_userService.CheckUserExists).WithMessage("User not exists");
            }
        }

        public class Handler : IRequestHandler<UpdateUserCommand, UserDto>
        {
            private readonly IUserService _userService;
            private readonly IMapper _mapper;

            public Handler(IUserService userService, IMapper mapper)
            {
                _userService = userService;
                _mapper = mapper;
            }
            /// <inheritdoc />
            public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
            {
                //throw new NotImplementedException("Implement a way to update the user associated with the provided Id.");
                User user = new User
                {
                    Id = request.Id,
                    GivenNames = request.GivenNames,
                    LastName = request.LastName,
                    ContactDetail = new ContactDetail
                    {
                        EmailAddress = request.EmailAddress,
                        MobileNumber = request.MobileNumber,
                        UserId = request.Id
                    }
                };

                User updatedUser = await _userService.UpdateAsync(user, cancellationToken);
                UserDto result = _mapper.Map<UserDto>(updatedUser);

                return result;
            }
        }
    }
}
