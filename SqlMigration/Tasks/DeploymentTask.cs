﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SqlMigration
{
    public class DeploymentTask : MigrationTask
    {
        private readonly IFileIO _fileIO;


        #region Constructors
        public DeploymentTask(Arguments arguments)
            : this(arguments, new FileIO(new FileIOWrapper())) //use structure map on this
        {
        }

        public DeploymentTask(Arguments args, IFileIO fileIO)
            : base(args)
        {
            _fileIO = fileIO;
        }

        #endregion

        public override int RunTask()
        {
            /// Steps to create deploy directory:
            /// 1. Get migrations neccessary to use (may need to filter on date)
            /// 2. Copy them into a folder that is passed in
            /// 3. Copy tool into folder
            /// 4. Create a batch file to run the console app?
            /// 


            /// Grab the migrations
            /// 1. get directory of scripts
            /// 2. get migrations

            List<Migration> migrations;
            string locationOfScripts = base.Arguments.GetArgumentValue("/sd");

            //parse for flag that says to include them
            bool includeTestData = base.Arguments.DoesArgumentExist("/t");

            //try to see if they want to filter the date
            string filterDateArg = Arguments.GetArgumentValue("/date");
            if(!string.IsNullOrEmpty(filterDateArg))
            {
                //filter it.. bool successParsing = false;
                bool successParsing = false;
                DateTime filterDate = DateParser.TryPrase(filterDateArg, out successParsing);
                //did it parse?
                if(!successParsing)
                    throw new ArgumentException("Could not parse the passed in filter date.");

                //get migrations based on a filter date
                migrations = _fileIO.GetMigrationsInOrder(locationOfScripts, includeTestData, filterDate);
            }
            else
            {
                //no filter date
                migrations = _fileIO.GetMigrationsInOrder(locationOfScripts, includeTestData);
            }


            /// Create location folder, and delete it if its there
            string locationToCopyTo = base.Arguments.GetArgumentValue("/cd");
            bool folderCreationSuccess = _fileIO.CreateFolder(locationToCopyTo);
            //todo: do something better if we failed
            if (!folderCreationSuccess)
                throw new Exception("Could not create the specified folder");


            //start copying them
            foreach (Migration migration in migrations)
            {
                bool copySuccess = _fileIO.CopyFile(migration.FilePath, locationToCopyTo);
                //todo: if it returns false do something
                if (!copySuccess) 
                    throw new Exception("Error copying file to location");
            }

            //copy the tool
            string exeLocation = Assembly.GetExecutingAssembly().Location;
            bool copyFile = _fileIO.CopyFile(exeLocation, locationToCopyTo);
            if (!copyFile) 
                throw new Exception("Error copying Migrator Tool");


            //todo: figure how create batch....

            return 0;
        }
    }
}