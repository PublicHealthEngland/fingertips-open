﻿using FingertipsUploadService.Upload;
using NLog;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace FingertipsUploadService.FpmFileReader
{
    public class ExcelFileReader : IUploadFileReader
    {
        private readonly Logger _logger = LogManager.GetLogger("ExcelFileReader");
        private string _filePath;

        public ExcelFileReader(string filePath)
        {
            _filePath = filePath;
        }

        /// <summary>
        /// Returns oleDbConnection on given file
        /// </summary>
        /// <param name="isFirstRowHeader"></param>
        /// <returns>OleDbConnection</returns>
        private OleDbConnection GetConnection(bool isFirstRowHeader)
        {
            _logger.Info("Opening file '{0}'", _filePath);

            var hdr = isFirstRowHeader ? "Yes" : "No";
            var connection = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + _filePath +
                       @";Extended Properties=""Excel 8.0;HDR=" + hdr + @";""");
            new OleDbCommand { Connection = connection };
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Returns the list of worksheet name
        /// </summary>
        /// <returns></returns>
        public List<string> GetWorksheets()
        {
            // Read worksheets
            var worksheetNames = new List<string>();

            var connection = GetConnection(false);
            var schema = connection.GetSchema("Tables");
            foreach (DataRow row in schema.Rows)
            {
                string sheetName = row["TABLE_NAME"].ToString();
                if (!sheetName.EndsWith("$") && !sheetName.EndsWith("$'"))
                    continue;
                worksheetNames.Add(sheetName);
            }

            connection.Dispose();

            return worksheetNames;
        }

        /// <summary>
        /// Reads indicatorDetails worksheet from simple upload file and  
        /// return it as DataTable
        /// </summary>
        /// <returns>DataTable</returns>
        public DataTable GetIndicatorDetails()
        {
            var indicatorDetails = GetDataTableFromWorksheet(WorksheetNames.SimpleIndicator, false, "indicatorDetails");
            return indicatorDetails;
        }

        /// <summary>
        /// Reads pholiodata worksheet from simple upload file and  
        /// return it as DataTable
        /// </summary>
        /// <returns>DataTable</returns>
        public DataTable GetPholioData()
        {
            var pholioData = GetDataTableFromWorksheet(WorksheetNames.SimplePholio, true, "indicatorDetails");
            return pholioData;
        }

        /// <summary>
        /// Reads indicator worksheet from batch upload file and  
        /// return it as DataTable
        /// </summary>
        /// <returns>DataTable</returns>
        public DataTable GetBatchData()
        {
            var batchData = GetDataTableFromWorksheet(WorksheetNames.BatchPholio, true, "results");
            return batchData;
        }

        /// <summary>
        /// Reads worksheet into datatable
        /// </summary>
        /// <param name="worksheetName"></param>
        /// <param name="isFirstRowHeader"></param>
        /// <param name="newDataTableName"></param>
        /// <returns>DataTable</returns>
        private DataTable GetDataTableFromWorksheet(string worksheetName, bool isFirstRowHeader, string newDataTableName)
        {
            OleDbConnection connection = null;
            OleDbDataAdapter adapter = null;

            connection = GetConnection(isFirstRowHeader);
            adapter = new OleDbDataAdapter(string.Format("SELECT * FROM [{0}]", worksheetName), connection);
            var ds = new DataSet();
            adapter.Fill(ds, newDataTableName);
            var dataTable = ds.Tables[newDataTableName];

            adapter.Dispose();
            connection.Dispose();
            connection.Close();

            return dataTable;
        }
    }
}