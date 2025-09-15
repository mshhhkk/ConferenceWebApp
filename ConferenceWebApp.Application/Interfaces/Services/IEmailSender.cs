﻿namespace ConferenceWebApp.Application.Interfaces.Services;

public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string body);
}