using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConferenceWebApp.Domain.Enums;

public enum ParticipantStatus
{
    Registered = 0,
    ProfileCompleted = 1,
    CheckSubmitted = 2,
    ParticipationConfirmed = 3
}
