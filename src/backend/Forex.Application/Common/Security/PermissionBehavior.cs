namespace Forex.Application.Common.Security;

using Forex.Application.Common.Exceptions;
using Forex.Application.Common.Interfaces;
using MediatR;

/// <summary>
/// So'rov turi <see cref="PermissionMap"/> da qayd etilgan bo'lsa, joriy foydalanuvchining
/// bo'lim ruxsatlari bitmask'ini tekshiradi. Kamida bitta talab qilingan bo'lim bit'i
/// bo'lmasa <see cref="ForbiddenException"/> (403) tashlaydi. admin doim All bo'lgani uchun o'tadi.
/// Xaritada yo'q so'rovlar har qanday autentifikatsiyalangan foydalanuvchi uchun ochiq.
/// </summary>
public class PermissionBehavior<TRequest, TResponse>(ICurrentUser currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        if (PermissionMap.Rules.TryGetValue(request.GetType(), out long required))
        {
            // OR semantikasi: talab qilingan bo'limlardan kamida bittasiga ruxsat bo'lishi kifoya.
            if ((currentUser.Permissions & required) == 0)
                throw new ForbiddenException("Sizda bu bo'limga kirish/amal bajarish huquqi yo'q.");
        }

        return await next();
    }
}
