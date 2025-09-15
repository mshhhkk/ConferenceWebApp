using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConferenceWebApp.Domain.Enums;
public enum ReportStatus
{
    SubmittedThesis = 0,
    ThesisApproved = 1,
    ThesisReturnedForCorrection = 2,
    SubmittedExtendedThesis = 3,
    ExtendedThesisApproved = 4,
    ExtendedThesisReturnedForCorrection = 5
}
