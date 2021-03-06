﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Commons.Collections;
using NVelocity;
using NVelocity.App;
using NVelocity.Runtime;
using SqlMigration.Contracts;

namespace SqlMigration.Tasks
{
    public class DeploymentTask : MigrationTask
    {
        public DeploymentTask(Arguments arguments) : base(arguments) { }

        public override int RunTask()
        {
            /// Steps to create deploy directory:
            /// 1. Get migrations neccessary to use (may need to filter on date)
            /// 2. loop over each one and each one of its commands and build up a file

            IList<Migration> migrations;
            string locationOfScripts = base.Arguments.GetArgumentValue(ArgumentConstants.ScriptDirectoryArg);

            //parse for flag that says to include them
            bool includeTestData = base.Arguments.DoesArgumentExist(ArgumentConstants.IncludeTestScriptsArg);

            //get migrations
            migrations = Factory.Get<IMigrationHelper>().GetMigrationsInOrder(locationOfScripts, includeTestData);

            //load up a velocity engine and run the template through it...
            string sqlOutput = CreateSqlOutput(migrations);

            //write file out
            string locationToCreateScript = base.Arguments.GetArgumentValue(TaskTypeConstants.DeploymentTask);
            Factory.Get<IFileIO>().WriteFile(locationToCreateScript, sqlOutput);

            return 0;
        }

        /// <summary>
        /// Creates a velocity engine that is initiated.  It loads up
        /// templates from the 'Templates' folder.
        /// </summary>
        /// <returns></returns>
        protected virtual string CreateSqlOutput(IEnumerable<Migration> migrations)
        {
            VelocityEngine velocityEngine = new VelocityEngine();

            ExtendedProperties extendedProperties = new ExtendedProperties();
            extendedProperties.SetProperty(RuntimeConstants.FILE_RESOURCE_LOADER_PATH, AppDomain.CurrentDomain.BaseDirectory + "\\Templates");

            velocityEngine.Init(extendedProperties);

            var context = new VelocityContext();
            context.Put("migrations", migrations.OrderBy(migration => migration.MigrationDate));

            var stringWriter = new StringWriter();
            velocityEngine.MergeTemplate("deployment_tsql.vm", "ISO-8859-1", context, stringWriter);

            return stringWriter.GetStringBuilder().ToString();
        }
    }
}
