﻿using System;

namespace yadd.core
{
    public abstract class Logger
    {
        public abstract void Write(string message);

        internal void ExportingTargetDatabaseSchema()
        {
            Write($"Exporting Target Database Schema");
        }

        internal void ScriptApplied(Job job)
        {
            Write($"Script {job.Name} applied");
        }

        internal void PreparingTargetDatabase()
        {
            Write($"Preparing Target Database");
        }

        internal void CleaningUp()
        {
            Write($"Cleaning up");
        }

        internal void AllDone()
        {
            Write($"All done.");
        }

        internal void ConnectingToTargetDatabase()
        {
            Write($"Connecting to Target Database.");
        }

        internal void ApplyingScript(Job job)
        {
            Write($"Applying {job.Name} Script");
        }
    }
}