using System.Data.SqlClient;
using Avant.Common.Utils;
using System;
using System.Configuration;

namespace avantMobile.Settings
{

	/// <summary>
	/// ��������� �������� ������������ ��� �������� ������ ���� ������.
	/// <see cref="avantMobile.Settings.AbstractSettingsProvider"/>
	/// </summary>
	public class DBSettingsProvider : AbstractSettingsProvider
	{
		
		/// <summary>
		/// �����������, ������ �����������.
		/// </summary>
		public DBSettingsProvider() 
		{ }

		/// <summary>
		/// ���� ������ �����������. �������� �� ������������. 
		/// ������ ����������� ����� ��� ���������� ��������� 
		/// SettingsProviderFactory.DefaultSettingsProvider
		/// </summary>
		/// <param name="source"></param>
		public DBSettingsProvider(string source)
			: base( source)
		{ }

		
		/// <summary>
		/// ��������� ������ �� ����
		/// </summary>
		protected override void LoadSettings()
		{
            using (SqlConnection con = Database.Connection)
            {
                string selectQuery = ConfigurationManager.AppSettings["DBSettingsProviderSelectQuery"] ?? Properties.Settings.Default.DBSettingsProviderSelectQuery;

                if (selectQuery.Contains("{0}") && string.IsNullOrEmpty(Instance))
                    return;

                selectQuery = string.Format(selectQuery, Instance);

                using ( SqlCommand cmd = new SqlCommand(selectQuery, con))
                {

                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        if (sdr == null) throw new ApplicationException("���������� ��������� ���������� ��������� �� ����.");
                        int ikey = sdr.GetOrdinal("key");
                        int ivalue = sdr.GetOrdinal("value");
                        int itype = sdr.GetOrdinal("type");

                        while(sdr.Read())
                        {
                            string key =sdr.GetString(ikey);
                            string strValue = sdr.GetString(ivalue);
                            string typeName = sdr.GetString(itype);
                            object tmpResult = ConvertFromString(strValue, typeName);
                            _InternalCollection.Add(key, tmpResult);
                        }

                    }
                }
            }
		}

		/// <summary>
		/// ��������� ������ � ����.
		/// ���� �� ��������.
		/// </summary>
		protected override void SaveSettings()
		{
			/*using( SqlConnection con = DBHelper.Connection )
			{
				ClearTable(con);

				StoreData(con);
			}*/
		}

        //private void StoreData(SqlConnection con)
        //{
        //    DataTable tbl = new DataTable("common.Settings");
        //    tbl.Columns.Add("key", typeof(string));
        //    tbl.Columns.Add("value", typeof(string));
        //    tbl.Columns.Add("type", typeof(string));

        //    foreach (string key in _InternalCollection.Keys)
        //    {
        //        object[] rowValues = new object[3];
        //        rowValues[0] = key;
        //        rowValues[1] = _InternalCollection[key].ToString();
        //        rowValues[2] = _InternalCollection[key].GetType().ToString();
        //        tbl.Rows.Add(rowValues);
        //    }

        //    SqlBulkCopy bcpy = new SqlBulkCopy(con);
        //    bcpy.BatchSize = 30;
        //    bcpy.DestinationTableName = "common.Settings";
        //    bcpy.ColumnMappings.Add(0, 1);
        //    bcpy.ColumnMappings.Add(1, 2);
        //    bcpy.ColumnMappings.Add(2, 3);
        //    bcpy.WriteToServer(tbl);
        //}

        //private void ClearTable(SqlConnection con)
        //{
        //    string delQuery = Properties.Settings.Default.DBSettingsProviderDelAllQuery;
        //    using (SqlCommand cmd = new SqlCommand(delQuery, con))
        //    {
        //        cmd.ExecuteNonQuery();
        //    }
        //}

	
	}
	
}
