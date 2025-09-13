namespace ConferenceWebApp.Domain.Exceptions;

public class ReportOperationException : Exception
{
    public ReportOperationException(string message) : base(message)
    {
    }
}

public class ReportAccessDeniedException : ReportOperationException
{
    public ReportAccessDeniedException() : base("Нет прав для выполнения операции")
    {
    }
}

public class ApprovedReportModificationException : ReportOperationException
{
    public ApprovedReportModificationException()
        : base("Нельзя изменять утвержденный отчет") { }
}