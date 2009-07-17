﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlMigration
{
    public static class TaskTypeFactory
    {
        /* Setup the intial values we have in our own codebase.  
             * People should be able to register different task strings
             * to actual types that implement MigrationTask.
             */
        private static readonly Dictionary<string, Type> _taskWithImplementationTypes =
            new Dictionary<string, Type>(3)
                {
                    {TaskTypeConstants.RunSqlFileTask, typeof(RunSqlFileTask)},
                    {TaskTypeConstants.DeploymentTask, typeof(DeploymentTask)},
                    {TaskTypeConstants.MigrateDatabaseForwardTask, typeof(MigrateDatabaseForwardTask)},
                };

        public static void Add(string taskCommand, Type implementationType)
        {
            _taskWithImplementationTypes.Add(taskCommand, implementationType);
        }

        public static Type GetTaskType(string taskType)
        {
            return _taskWithImplementationTypes.Where(pair => pair.Key == taskType).Single().Value;
        }
    }
}