﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Core.Utils.Logger
{
    public enum LogLevel
    {
        //
        // Summary:
        //     Logs that contain the most detailed messages. These messages may contain sensitive
        //     application data. These messages are disabled by default and should never be
        //     enabled in a production environment.
        Trace,
        //
        // Summary:
        //     Logs that are used for interactive investigation during development. These logs
        //     should primarily contain information useful for debugging and have no long-term
        //     value.
        Debug,
        //
        // Summary:
        //     Logs that track the general flow of the application. These logs should have long-term
        //     value.
        Information,
        //
        // Summary:
        //     Logs that highlight an abnormal or unexpected event in the application flow,
        //     but do not otherwise cause the application execution to stop.
        Warning,
        //
        // Summary:
        //     Logs that highlight when the current flow of execution is stopped due to a failure.
        //     These should indicate a failure in the current activity, not an application-wide
        //     failure.
        Error,
        //
        // Summary:
        //     Logs that describe an unrecoverable application or system crash, or a catastrophic
        //     failure that requires immediate attention.
        Critical,
        //
        // Summary:
        //     Not used for writing log messages. Specifies that a logging category should not
        //     write any messages.
        None
    }
}
