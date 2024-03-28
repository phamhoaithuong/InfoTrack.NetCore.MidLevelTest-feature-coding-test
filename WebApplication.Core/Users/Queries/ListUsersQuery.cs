using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using WebApplication.Core.Common.Models;
using WebApplication.Core.Users.Common.Models;
using WebApplication.Infrastructure.Interfaces;

namespace WebApplication.Core.Users.Queries
{
    public class ListUsersQuery : IRequest<PaginatedDto<IEnumerable<UserDto>>>
    {
        public int PageNumber { get; set; }
        public int ItemsPerPage { get; set; } = 10;

        public class Validator : AbstractValidator<ListUsersQuery>
        {
            public Validator()
            {
                // TODO: Create a validation rule so that PageNumber is always greater than 0
                RuleFor(x => x.PageNumber)
                    .GreaterThan(0)
                    .WithMessage("PageNumber is must be greater than 0");
            }
        }

        public class Handler : IRequestHandler<ListUsersQuery, PaginatedDto<IEnumerable<UserDto>>>
        {
            /// <inheritdoc />
            /// Implement a way to get a paginated list of all the users in the database.   
            /// 

            private readonly IUserService _userService;
            private readonly IMapper _mapper;

            public Handler(IUserService userService, IMapper mapper)
            {
                _userService = userService;
                _mapper = mapper;
            }
            public async Task<PaginatedDto<IEnumerable<UserDto>>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
            {
                var users = await _userService.GetPaginatedAsync(request.PageNumber, request.ItemsPerPage,cancellationToken);
                var totalRecords = await _userService.CountAsync(cancellationToken);

                var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
                var paginatedDto = new PaginatedDto<IEnumerable<UserDto>>();
                paginatedDto.Data = userDtos;
                paginatedDto.HasNextPage = request.PageNumber < ((int)Math.Ceiling(totalRecords / (double)request.ItemsPerPage));
                return paginatedDto;
            }
        }
    }
}
