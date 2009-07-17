﻿using System;
using System.Collections.Generic;

namespace SqlMigration
{
    public class MigrateDatabaseForwardTask : MigrationTask
    {
        private readonly IFileIO _fileIO;
        private readonly ISqlRunner _sqlRunner;

        #region Constructors

        public MigrateDatabaseForwardTask(Arguments arguments, IFileIO fileIO, ISqlRunner sqlRunner)
            : base(arguments)
        {
            _fileIO = fileIO;
            _sqlRunner = sqlRunner;
        }

        #endregion

        public override int RunTask()
        {
            int success = -1;
            
            //grab migrations
            string scriptDirectory = Arguments.GetArgumentValue(ArgumentConstants.ScriptDirectoryArg);
            //make it look for a flag to include test data
            bool includeTestData = Arguments.DoesArgumentExist(ArgumentConstants.IncludeTestScriptsArg);
            IList<Migration> migartions = _fileIO.GetMigrationsInOrder(scriptDirectory, includeTestData); 
            

            //grab connection string
            string connectionString = base.Arguments.GetArgumentValue(ArgumentConstants.ConnectionStringArg);
            _sqlRunner.ConnectionString = connectionString;

            //find if we want to run inside transaction...
            bool runInsideTransaction = !base.Arguments.DoesArgumentExist(ArgumentConstants.RunWithoutTransactionArg);

            //run the migrations
            int successRunningSql = _sqlRunner.StartMigrations(migartions, runInsideTransaction, true); 
            success = successRunningSql;

            return success;
        }
    }
}
