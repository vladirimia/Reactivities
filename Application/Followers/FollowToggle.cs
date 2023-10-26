using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Followers;

public class FollowToggle
{
    public class Command : IRequest<Result<Unit>>
    {
        public string TargetUsername { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _dataContext;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext dataContext, IUserAccessor userAccessor)
        {
            _dataContext = dataContext;
            _userAccessor = userAccessor;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var observer = await _dataContext.Users.FirstOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername());
            var target = await _dataContext.Users.FirstOrDefaultAsync(x => x.UserName == request.TargetUsername);

            if (target is null)
            {
                return null;
            }

            var following = await _dataContext.UserFollowings.FindAsync(observer.Id, target.Id);

            if (following is null)
            {
                following = new UserFollowing
                {
                    Target = target,
                    Observer = observer
                };

                _dataContext.UserFollowings.Add(following);
            }
            else
            {
                _dataContext.UserFollowings.Remove(following);
            }

            var result = await _dataContext.SaveChangesAsync() > 0;

            if (result)
            {
                return Result<Unit>.Success(Unit.Value);
            }

            return Result<Unit>.Failure("Failed to update following");
        }
    }
}
