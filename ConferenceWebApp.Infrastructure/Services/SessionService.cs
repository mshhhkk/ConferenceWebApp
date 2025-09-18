using ConferenceWebApp.Application.DTOs.PersonalAccountDTOs;
using ConferenceWebApp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConferenceWebApp.Infrastructure.Services;

public class SessionService : ISessionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SessionService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void SaveSession(string key, object value)
    {
        var serializedValue = JsonConvert.SerializeObject(value);
        _httpContextAccessor.HttpContext.Session.SetString(key, serializedValue);
    }

    // Получение данных из сессии
    public T GetSession<T>(string key)
    {
        var sessionValue = _httpContextAccessor.HttpContext.Session.GetString(key);
        return string.IsNullOrEmpty(sessionValue) ? default : JsonConvert.DeserializeObject<T>(sessionValue);
    }

    // Удаление данных из сессии
    public void DeleteSession(string key)
    {
        _httpContextAccessor.HttpContext.Session.Remove(key);
    }
    public void UpdateSession(string key, object newValue)
    {
   
        SaveSession(key, newValue);
    }

}
