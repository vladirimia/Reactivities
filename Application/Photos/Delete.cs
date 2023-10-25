﻿using Application.Core;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Photos;

public class Delete
{
    public class Command : IRequest<Result<Unit>>
    {
        public string Id { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _dataContext;
        private readonly IUserAccessor _userAccessor;
        private readonly IPhotoAccessor _photoAccessor;

        public Handler(
            DataContext dataContext,
            IUserAccessor userAccessor,
            IPhotoAccessor photoAccessor)
        {
            _dataContext = dataContext;
            _userAccessor = userAccessor;
            _photoAccessor = photoAccessor;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _dataContext.Users
                .Include(p => p.Photos)
                .FirstOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername());

            if (user is null)
            {
                return null;
            }

            var photo = user.Photos.FirstOrDefault(x => x.Id == request.Id);

            if (photo is null)
            {
                return null;
            }

            if (photo.IsMain)
            {
                return Result<Unit>.Failure("You cannot delete your main photo");
            }

            var result = await _photoAccessor.DeletePhoto(photo.Id);

            if (result is null)
            {
                return Result<Unit>.Failure("Error deleting photo from Cloudinary");
            }

            user.Photos.Remove(photo);

            var success = await _dataContext.SaveChangesAsync() > 0;

            if (success)
            {
                return Result<Unit>.Success(Unit.Value);
            }

            return Result<Unit>.Failure("Error deleting photo from API");
        }
    }
}
