// Application/Interfaces/Services/IInvitationImageService.cs
using System;
using System.Threading.Tasks;

namespace ConferenceWebApp.Application.Interfaces.Services
{
    public interface IInvitationImageService
    {
        /// <summary>Строит PNG-приглашение для пользователя.</summary>
        Task<Result<InvitationPng>> BuildForUserAsync(Guid userId, CancellationToken ct = default);
    }

    public sealed record InvitationPng(byte[] Bytes);
}
