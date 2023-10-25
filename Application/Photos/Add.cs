﻿using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Photos;

public class Add
{
    public class Command : IRequest<Result<Photo>>
    {
        public IFormFile File { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<Photo>>
    {
        private readonly DataContext _dataContext;
        private readonly IPhotoAccessor _photoAccessor;
        private readonly IUserAccessor _userAccessor;

        public Handler(
            DataContext dataContext,
            IPhotoAccessor photoAccessor,
            IUserAccessor userAccessor)
        {
            _dataContext = dataContext;
            _photoAccessor = photoAccessor;
            _userAccessor = userAccessor;
        }

        public async Task<Result<Photo>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _dataContext.Users
                .Include(p => p.Photos)
                .FirstOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername());

            if (user is null)
            {
                return null;
            }

            var photoUploadResult = await _photoAccessor.AddPhoto(request.File);
            var photo = new Photo
            {
                Url = photoUploadResult.Url,
                Id = photoUploadResult.PublicId,
                IsMain = !user.Photos.Any(x => x.IsMain)
            };

            user.Photos.Add(photo);

            var result = await _dataContext.SaveChangesAsync() > 0;

            if (result)
            {
                return Result<Photo>.Success(photo);
            }

            return Result<Photo>.Failure("Problem adding photo");
        }
    }
}