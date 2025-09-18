using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConferenceWebApp.Application.Interfaces.Services;

public interface ISessionService
{
    void SaveSession(string key, object value);
    T GetSession<T>(string key);
    void DeleteSession(string key);
    void UpdateSession(string key, object newValue);
}
