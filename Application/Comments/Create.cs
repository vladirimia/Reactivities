using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Comments;

public class Create
{
    public class Command : IRequest<Result<CommentDto>>
    {
        public string Body { get; set; }
        public Guid ActivityId { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.Body).NotEmpty();
        }
    }

    public class Handler : IRequestHandler<Command, Result<CommentDto>>
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext dataContext, IMapper mapper, IUserAccessor userAccessor)
        {
            _dataContext = dataContext;
            _mapper = mapper;
            _userAccessor = userAccessor;
        }

        public async Task<Result<CommentDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var activity = await _dataContext.Activities.FindAsync(request.ActivityId);

            if (activity is null)
            {
                return null;
            }

            var user = await _dataContext.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername());

            if (user is null)
            {
                return null;
            }

            var comment = new Comment
            {
                Author = user,
                Activity = activity,
                Body = request.Body
            };

            activity.Comments.Add(comment);

            var result = await _dataContext.SaveChangesAsync() > 0;

            if (result)
            {
                Result<CommentDto>.Success(_mapper.Map<CommentDto>(comment));
            }

            return Result<CommentDto>.Failure("Failed to add comment");
        }
    }
}
